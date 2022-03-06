using AAPTForNet.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace AAPTForNet
{
    /// <summary>
    /// Android Assert Packing Tool for NET
    /// </summary>
    public class AAPTool : Process
    {
        private enum DumpTypes
        {
            Manifest = 0,
            Resources = 1,
            XmlTree = 2,
        }

        private static readonly string AppPath = Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location);
#if NET5_0_OR_GREATER && !NETCOREAPP
        private static readonly string TempPath = Path.Combine(Path.GetTempPath(), @"APKInstaller\Caches", $"{Environment.ProcessId}", "AppPackages");
#else
        private static readonly string TempPath = Path.Combine(Path.GetTempPath(), @"APKInstaller\Caches", $"{GetCurrentProcess().Id}", "AppPackages");
#endif

        protected AAPTool()
        {
            StartInfo.FileName = AppPath + @"\tool\aapt.exe";
            StartInfo.CreateNoWindow = true;
            StartInfo.UseShellExecute = false; // For read output data
            StartInfo.RedirectStandardError = true;
            StartInfo.RedirectStandardOutput = true;
            StartInfo.StandardOutputEncoding = System.Text.Encoding.GetEncoding("utf-8");
        }

        protected new bool Start(string args)
        {
            StartInfo.Arguments = args;
            return base.Start();
        }

        private static DumpModel dump(
            string path,
            string args,
            DumpTypes type,
            Func<string, int, bool> callback)
        {

            int index = 0;
            bool terminated = false;
            string msg = string.Empty;
            AAPTool aapt = new AAPTool();
            List<string> output = new List<string>();    // Messages from output stream

            switch (type)
            {
                case DumpTypes.Manifest:
                    aapt.Start($"dump badging \"{path}\"");
                    break;
                case DumpTypes.Resources:
                    aapt.Start($"dump --values resources \"{path}\"");
                    break;
                case DumpTypes.XmlTree:
                    aapt.Start($"dump xmltree \"{path}\" {args}");
                    break;
                default:
                    return new DumpModel(path, false, output);
            }

            while (!aapt.StandardOutput.EndOfStream && !terminated)
            {
                msg = aapt.StandardOutput.ReadLine();

                if (callback(msg, index))
                {
                    terminated = true;
                    try
                    {
                        aapt.Kill();
                    }
                    catch { }
                }
                if (!terminated)
                {
                    index++;
                }

                output.Add(msg);
            }

            while (!aapt.StandardError.EndOfStream)
            {
                output.Add(aapt.StandardError.ReadLine());
            }

            try
            {
                aapt.WaitForExit();
                aapt.Close();
            }
            catch { }

            // Dump xml tree get only 1 message when failed, the others are 2.
            bool isSuccess = type != DumpTypes.XmlTree ?
                output.Count > 2 : output.Count > 0;
            return new DumpModel(path, isSuccess, output);
        }

        internal static DumpModel dumpManifest(string path)
        {
            return dump(path, string.Empty, DumpTypes.Manifest, (msg, i) => false);
        }

        internal static DumpModel dumpResources(string path, Func<string, int, bool> callback)
        {
            return dump(path, string.Empty, DumpTypes.Resources, callback);
        }

        internal static DumpModel dumpXmlTree(string path, string asset, Func<string, int, bool> callback = null)
        {
            callback = callback ?? ((_, __) => false);
            return dump(path, asset, DumpTypes.XmlTree, callback);
        }

        internal static DumpModel dumpManifestTree(string path, Func<string, int, bool> callback = null)
        {
            return dumpXmlTree(path, "AndroidManifest.xml", callback);
        }

        /// <summary>
        /// Start point. Begin decompile apk to extract resources
        /// </summary>
        /// <param name="path">Absolute path to .apk file</param>
        /// <returns>Filled apk if dump process is not failed</returns>
        public static ApkInfo Decompile(string path)
        {
            List<string> apks = new List<string>();
            using (ZipArchive archive = ZipFile.OpenRead(path))
            {
                if (!Directory.Exists(TempPath))
                {
                    Directory.CreateDirectory(TempPath);
                }

                foreach (ZipArchiveEntry entry in archive.Entries.Where(x => !x.FullName.Contains("/")))
                {
                    if (entry.Name.ToLower().EndsWith(".apk"))
                    {
                        string APKTemp = Path.Combine(TempPath, entry.FullName);
                        entry.ExtractToFile(APKTemp, true);
                        apks.Add(APKTemp);
                    }
                }

                if (!apks.Any())
                {
                    apks.Add(path);
                }
            }

            List<ApkInfo> apkInfos = new List<ApkInfo>();
            foreach (string apkpath in apks)
            {
                DumpModel manifest = ApkExtractor.ExtractManifest(apkpath);
                if (!manifest.isSuccess)
                {
                    continue;
                }

                ApkInfo apk = ApkParser.Parse(manifest);
                apk.FullPath = apkpath;

                if (apk.Icon.isImage)
                {
                    // Included icon in manifest, extract it from apk
                    apk.Icon.RealPath = ApkExtractor.ExtractIconImage(apkpath, apk.Icon);
                    if (apk.Icon.isHighDensity)
                    {
                        apkInfos.Add(apk);
                        continue;
                    }
                }

                apk.Icon = ApkExtractor.ExtractLargestIcon(apkpath);
                apkInfos.Add(apk);
            }

            if (!apkInfos.Any()) { return new ApkInfo(); }

            if (apkInfos.Count <= 1) { return apkInfos.First(); }

            List<ApkInfos> packages = apkInfos.GroupBy(x => x.PackageName).Select(x => new ApkInfos { PackageName = x.Key, Apks = x.ToList() }).ToList();

            if (packages.Count > 1) { throw new Exception("This is a Multiple Package."); }

            List<ApkInfo> infos = new List<ApkInfo>();
            foreach (ApkInfos package in packages)
            {
                foreach (ApkInfo baseapk in package.Apks.Where(x => !x.IsSplit))
                {
                    baseapk.SplitApks = package.Apks.Where(x => x.IsSplit).Where(x => x.VersionCode == baseapk.VersionCode).ToList();
                    infos.Add(baseapk);
                }
            }

            if (infos.Count > 1) { throw new Exception("There are more than one base APK in this Package."); }

            if (!infos.Any()) { throw new Exception("There are all dependents in this Package."); }

            ApkInfo info = infos.First();

            return info;
        }
    }
}
