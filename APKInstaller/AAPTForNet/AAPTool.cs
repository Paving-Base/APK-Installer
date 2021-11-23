using AAPTForNet.Models;

using System;
using System.Collections.Generic;
using System.IO;

namespace AAPTForNet
{
    /// <summary>
    /// Android Assert Packing Tool for NET
    /// </summary>
    public class AAPTool : System.Diagnostics.Process
    {
        private enum DumpTypes
        {
            Manifest = 0,
            Resources = 1,
            XmlTree = 2,
        }

        private static readonly string AppPath = Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location);

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
            DumpModel manifest = ApkExtractor.ExtractManifest(path);
            if (!manifest.isSuccess)
            {
                return new ApkInfo();
            }

            ApkInfo apk = ApkParser.Parse(manifest);
            apk.FullPath = path;

            if (apk.Icon.isImage)
            {
                // Included icon in manifest, extract it from apk
                apk.Icon.RealPath = ApkExtractor.ExtractIconImage(path, apk.Icon);
                if (apk.Icon.isHighDensity)
                {
                    return apk;
                }
            }

            apk.Icon = ApkExtractor.ExtractLargestIcon(path);
            return apk;
        }
    }
}
