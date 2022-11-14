using AAPTForNet.Models;

namespace AAPTForNet.Filters
{
    internal class SupportScrFilter : BaseFilter
    {
        public const string SmallScreen = "small";
        public const string NormalScreen = "normal";
        public const string LargeScreen = "large";
        public const string xLargeScreen = "xlarge";

        private string Message = string.Empty;

        public override bool CanHandle(string msg) => msg.StartsWith("supports-screens:");

        public override void AddMessage(string msg) => Message = msg;

        public override ApkInfo GetAPK()
        {
            ApkInfo apk = new();

            if (Message.Contains(SmallScreen))
            {
                apk.SupportScreens.Add(SmallScreen);
            }

            if (Message.Contains(NormalScreen))
            {
                apk.SupportScreens.Add(NormalScreen);
            }

            if (Message.Contains(LargeScreen))
            {
                apk.SupportScreens.Add(LargeScreen);
            }

            if (Message.Contains(xLargeScreen))
            {
                apk.SupportScreens.Add(xLargeScreen);
            }

            return apk;
        }

        public override void Clear() => Message = string.Empty;
    }
}
