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
        private string[] Segments = Array.Empty<string>();

        public override bool CanHandle(string msg) => msg.StartsWith("native-code:");

        public override void AddMessage(string msg) => Segments = msg.Split(new char[2] { ' ', '\'' }, StringSplitOptions.RemoveEmptyEntries);

        public override ApkInfo GetAPK() => new() { SupportedABIs = Segments.Skip(1).ToList() }; // Skip "native-code"        

        public override void Clear() => throw new NotImplementedException();
    }
}
