using AAPTForNet.Models;

namespace AAPTForNet.Filters
{
    internal class PackageFilter : BaseFilter
    {

        private string[] Segments = new string[] { };

        public override bool CanHandle(string msg)
        {
            return msg.StartsWith("package:");
        }

        public override void AddMessage(string msg)
        {
            Segments = msg.Split(Seperator);
        }

        public override ApkInfo GetAPK()
        {
            return new ApkInfo()
            {
                SplitName = getValueOrDefault("split"),
                PackageName = getValueOrDefault("package"),
                VersionName = getValueOrDefault("versionName"),
                VersionCode = getValueOrDefault("versionCode"),
            };
        }

        public override void Clear() => Segments = new string[] { };

        private string getValueOrDefault(string key)
        {
            string output = string.Empty;
            for (int i = 0; i < Segments.Length; i++)
            {
                if (Segments[i].Contains(key))
                {    // Find key
                    output = Segments[++i];         // Get value
                    break;
                }
            }
            return string.IsNullOrEmpty(output) ? DefaultEmptyValue : output;
        }
    }
}
