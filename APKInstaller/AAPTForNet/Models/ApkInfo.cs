using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AAPTForNet.Models
{
    public class ApkInfo
    {
        public string AppName { get; set; } = string.Empty;
        public string SplitName { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string VersionName { get; set; } = string.Empty;
        public string VersionCode { get; set; } = string.Empty;

        /// <summary>
        /// Absolute path to apk file
        /// </summary>
        public string FullPath { get; set; } = string.Empty;
        public string PackagePath { get; set; } = string.Empty;
        public string LaunchableActivity { get; set; } = string.Empty;
        public Icon Icon { get; set; } = Icon.Default;
        public SDKInfo MinSDK { get; set; } = SDKInfo.Unknown;
        public SDKInfo TargetSDK { get; set; } = SDKInfo.Unknown;
        public List<ApkInfo> SplitApks { get; set; } = new();
        public List<string> Features { get; set; } = new();
        public List<string> Permissions { get; set; } = new();

        /// <summary>
        /// Supported application binary interfaces
        /// </summary>
        public List<string> SupportedABIs { get; set; } = new();
        public List<string> SupportLocales { get; set; } = new();
        public List<string> SupportScreens { get; set; } = new();
        public List<string> SupportDensities { get; set; } = new();
        public Dictionary<string, string> LocaleLabels { get; set; } = new();

        /// <summary>
        /// Size of package, in bytes
        /// </summary>
        public long PackageSize
        {
            get
            {
                try
                {
                    return new FileInfo(FullPath).Length;
                }
                catch
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Determines whether this package is filled or not
        /// </summary>
        public bool IsEmpty => AppName == string.Empty && PackageName == string.Empty;

        public bool IsSplit => SplitName != "Unknown";

        public bool IsBundle => SplitApks != null && SplitApks.Any();

        public void AddSplit(ApkInfo info) => SplitApks.Add(info);

        public void AddSplit(string path) => SplitApks.Add(AAPTool.Decompile(path));

        internal ApkInfo Megre(params ApkInfo[] apks) => apks.Any(a => a == null) ? throw new ArgumentNullException(nameof(apks)) : Merge(this, apks);

        internal static ApkInfo Merge(IEnumerable<ApkInfo> apks) => Merge(null, apks);

        internal static ApkInfo Merge(ApkInfo init, IEnumerable<ApkInfo> apks)
        {
            init ??= new ApkInfo();

            ApkInfo appApk = apks.FirstOrDefault(a => a.AppName.Length > 0);
            if (appApk != null)
            {
                init.AppName = appApk.AppName;
            }

            ApkInfo pckApk = apks.FirstOrDefault(a => a.PackageName.Length > 0);
            if (pckApk != null)
            {
                init.SplitName = pckApk.SplitName;
                init.VersionName = pckApk.VersionName;
                init.VersionCode = pckApk.VersionCode;
                init.PackageName = pckApk.PackageName;
            }

            ApkInfo lauApk = apks.FirstOrDefault(a => a.LaunchableActivity.Length > 0);
            if (lauApk != null)
            {
                init.LaunchableActivity = lauApk.LaunchableActivity;
            }

            ApkInfo sdkApk = apks.FirstOrDefault(a => !SDKInfo.Unknown.Equals(a.MinSDK));
            if (sdkApk != null)
            {
                init.MinSDK = sdkApk.MinSDK;
                init.TargetSDK = sdkApk.TargetSDK;
            }

            ApkInfo feaApk = apks.FirstOrDefault(a => a.Features.Count > 0);
            if (feaApk != null)
            {
                init.Features = feaApk.Features;
            }

            ApkInfo perApk = apks.FirstOrDefault(a => a.Permissions.Count > 0);
            if (perApk != null)
            {
                init.Permissions = perApk.Permissions;
            }

            ApkInfo labApk = apks.FirstOrDefault(a => a.LocaleLabels.Count > 0);
            if (labApk != null)
            {
                init.LocaleLabels = labApk.LocaleLabels;
            }

            ApkInfo abiApk = apks.FirstOrDefault(a => a.SupportedABIs.Count > 0);
            if (abiApk != null)
            {
                init.SupportedABIs = abiApk.SupportedABIs;
            }

            ApkInfo locApk = apks.FirstOrDefault(a => a.SupportLocales.Count > 0);
            if (locApk != null)
            {
                init.SupportLocales = locApk.SupportLocales;
            }

            ApkInfo scrApk = apks.FirstOrDefault(a => a.SupportScreens.Count > 0);
            if (scrApk != null)
            {
                init.SupportScreens = scrApk.SupportScreens;
            }

            ApkInfo denApk = apks.FirstOrDefault(a => a.SupportDensities.Count > 0);
            if (denApk != null)
            {
                init.SupportDensities = denApk.SupportDensities;
            }

            ApkInfo iconApk = apks.FirstOrDefault(a => !Icon.Default.Equals(a.Icon));
            if (iconApk != null)
            {
                init.Icon = iconApk.Icon;
            }

            ApkInfo pathApk = apks.FirstOrDefault(a => a.FullPath.Length > 0);
            if (pathApk != null)
            {
                init.FullPath = pathApk.FullPath;
            }

            if (init.IsSplit)
            {
                if (init.AppName == "Unknown" && init.SplitName != "Unknown")
                {
                    init.AppName = init.SplitName;
                }
                if (init.VersionName == "Unknown" && init.VersionCode != "Unknown")
                {
                    init.VersionName = init.VersionCode;
                }
            }

            return init;
        }
    }

    internal class ApkInfos
    {
        public string PackageName { get; set; }
        public string VersionCode { get; set; }
        public List<ApkInfo> Apks { get; set; }
    }
}
