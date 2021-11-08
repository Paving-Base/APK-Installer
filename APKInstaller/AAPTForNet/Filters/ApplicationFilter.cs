using AAPTForNet.Models;

namespace AAPTForNet.Filters
{
    internal class ApplicationFilter : BaseFilter
    {

        private string[] segments = new string[] { };

        public override bool canHandle(string msg)
        {
            return msg.StartsWith("application:");
        }

        public override void addMessage(string msg = "")
        {
            segments = msg.Split(seperator);
        }

        public override ApkInfo getAPK()
        {
            // Try getting icon name from manifest, may be an image
            string iconName = getValue("icon=");

            return new ApkInfo()
            {
                AppName = getValue("label="),
                Icon = iconName == defaultEmptyValue ?
                    Icon.Default : new Icon(iconName)
            };
        }

        public override void clear() => segments = new string[] { };

        private string getValue(string key)
        {
            string output = string.Empty;
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i].Contains(key))
                {
                    output = segments[++i];
                    break;
                }
            }
            return string.IsNullOrEmpty(output) ? defaultEmptyValue : output;
        }
    }
}
