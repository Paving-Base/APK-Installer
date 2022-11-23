using AAPTForNet.Models;
using System;

namespace AAPTForNet.Filters
{
    internal class PackageFilter : BaseFilter
    {
        private string[] Segments = Array.Empty<string>();

        public override bool CanHandle(string msg) => msg.StartsWith("package:");

        public override void AddMessage(string msg) => Segments = msg.Split(Seperator);

        public override ApkInfo GetAPK()
        {
            return new ApkInfo
            {
                SplitName = GetValueOrDefault("split"),
                PackageName = GetValueOrDefault("package"),
                VersionName = GetValueOrDefault("versionName"),
                VersionCode = GetValueOrDefault("versionCode"),
            };
        }

        public override void Clear() => Segments = Array.Empty<string>();

        private string GetValueOrDefault(string key)
        {
            string output = string.Empty;
            for (int i = 0; i < Segments.Length; i++)
            {
                if (Segments[i].Contains(key))
                {
                    // Find key
                    output = Segments[++i]; // Get value
                    break;
                }
            }
            return string.IsNullOrEmpty(output) ? DefaultEmptyValue : output;
        }
    }
}
