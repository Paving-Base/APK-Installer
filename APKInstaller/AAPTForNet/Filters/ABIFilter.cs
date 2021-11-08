using AAPTForNet.Models;
using System;
using System.Linq;

namespace AAPTForNet.Filters
{
    /// <summary>
    /// Application Binary Interface Filter
    /// </summary>
    /// <remarks>https://developer.android.com/ndk/guides/abis</remarks>
    internal class ABIFilter : BaseFilter
    {

        private string[] segments = new string[] { };

        public override bool canHandle(string msg)
            => msg.StartsWith("native-code:");

        public override void addMessage(string msg)
        {
            segments = msg.Split(new char[2] { ' ', '\'' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public override ApkInfo getAPK()
        {
            return new ApkInfo()
            {
                SupportedABIs = segments.Skip(1).ToList()   // Skip "native-code"
            };
        }

        public override void clear() => throw new NotImplementedException();
    }
}
