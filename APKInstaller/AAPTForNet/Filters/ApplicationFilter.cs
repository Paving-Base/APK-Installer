using AAPTForNet.Models;

namespace AAPTForNet.Filters
{
    internal class ApplicationFilter : BaseFilter
    {
        private string[] Segments = new string[] { };

        public override bool CanHandle(string msg)
        {
            return msg.StartsWith("application:");
        }

        public override void AddMessage(string msg = "")
        {
            Segments = msg.Split(Seperator);
        }

        public override ApkInfo GetAPK()
        {
            // Try getting icon name from manifest, may be an image
            string iconName = GetValue("icon=");

            return new ApkInfo()
            {
                AppName = GetValue("label="),
                Icon = iconName == DefaultEmptyValue ?
                    Icon.Default : new Icon(iconName)
            };
        }

        public override void Clear() => Segments = new string[] { };

        private string GetValue(string key)
        {
            string output = string.Empty;
            for (int i = 0; i < Segments.Length; i++)
            {
                if (Segments[i].Contains(key))
                {
                    output = Segments[++i];
                    break;
                }
            }
            return string.IsNullOrEmpty(output) ? DefaultEmptyValue : output;
        }
    }
}
