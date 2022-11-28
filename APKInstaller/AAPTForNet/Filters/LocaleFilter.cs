using AAPTForNet.Models;
using System;
using System.Linq;

namespace AAPTForNet.Filters
{
    internal class LocaleFilter : BaseFilter
    {
        private string[] Segments = Array.Empty<string>();

        public override bool CanHandle(string msg) => msg.StartsWith("locales: '--_--'");

        public override void AddMessage(string msg) => Segments = msg.Split(new char[2] { ' ', '\'' }, StringSplitOptions.RemoveEmptyEntries);

        public override ApkInfo GetAPK() => new() { SupportLocales = Segments.Skip(2).ToList() }; // Skip "locales" and "--_--"     

        public override void Clear() => throw new NotImplementedException();
    }
}
