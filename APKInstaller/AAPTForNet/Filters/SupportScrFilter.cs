using AAPTForNet.Models;

namespace AAPTForNet.Filters
{
    internal class SupportScrFilter : BaseFilter
    {

        public const string SmallScreen = "small";
        public const string NormalScreen = "normal";
        public const string LargeScreen = "large";
        public const string xLargeScreen = "xlarge";

        private string msg = string.Empty;

        public override bool canHandle(string msg)
        {
            return msg.StartsWith("supports-screens:");
        }

        public override void addMessage(string msg)
        {
            this.msg = msg;
        }

        public override ApkInfo getAPK()
        {
            ApkInfo apk = new ApkInfo();

            if (msg.Contains(SmallScreen))
            {
                apk.SupportScreens.Add(SmallScreen);
            }

            if (msg.Contains(NormalScreen))
            {
                apk.SupportScreens.Add(NormalScreen);
            }

            if (msg.Contains(LargeScreen))
            {
                apk.SupportScreens.Add(LargeScreen);
            }

            if (msg.Contains(xLargeScreen))
            {
                apk.SupportScreens.Add(xLargeScreen);
            }

            return apk;
        }

        public override void clear() => msg = string.Empty;
    }
}
