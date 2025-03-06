using AAPTForNet.Models;
using System;
using System.Linq;

namespace AAPTForNet.Filters
{
    internal class LocaleFilter : BaseFilter
    {
        private string[] Segments = [];

        public override bool CanHandle(string msg) => msg.StartsWith("locales: '--_--'");

        public override void AddMessage(string msg) => Segments = msg.Split([' ', '\''], StringSplitOptions.RemoveEmptyEntries);

        public override ApkInfo GetAPK() => new() { SupportLocales = [.. Segments.Skip(2)] }; // Skip "locales" and "--_--"     

        public override void Clear() => throw new NotImplementedException();
    }
}
