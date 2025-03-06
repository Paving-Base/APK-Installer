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
        private string[] Segments = [];

        public override bool CanHandle(string msg) => msg.StartsWith("native-code:");

        public override void AddMessage(string msg) => Segments = msg.Split([' ', '\''], StringSplitOptions.RemoveEmptyEntries);

        public override ApkInfo GetAPK() => new() { SupportedABIs = [.. Segments.Skip(1)] }; // Skip "native-code"        

        public override void Clear() => throw new NotImplementedException();
    }
}
