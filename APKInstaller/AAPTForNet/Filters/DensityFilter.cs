using AAPTForNet.Models;
using System;
using System.Linq;

namespace AAPTForNet.Filters
{
    internal class DensityFilter : BaseFilter
    {
        private string[] Segments = Array.Empty<string>();

        public override bool CanHandle(string msg) => msg.StartsWith("densities:");

        public override void AddMessage(string msg) => Segments = msg.Split(new char[2] { ' ', '\'' }, StringSplitOptions.RemoveEmptyEntries);

        public override ApkInfo GetAPK() => new() { SupportDensities = Segments.Skip(1).ToList() }; // Skip "densities"        

        public override void Clear() => throw new NotImplementedException();
    }
}
