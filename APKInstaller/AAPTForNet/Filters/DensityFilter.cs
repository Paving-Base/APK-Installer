using AAPTForNet.Models;
using System;
using System.Linq;

namespace AAPTForNet.Filters
{
    internal class DensityFilter : BaseFilter
    {
        private string[] Segments = [];

        public override bool CanHandle(string msg) => msg.StartsWith("densities:");

        public override void AddMessage(string msg) => Segments = msg.Split([' ', '\''], StringSplitOptions.RemoveEmptyEntries);

        public override ApkInfo GetAPK() => new() { SupportDensities = [.. Segments.Skip(1)] }; // Skip "densities"        

        public override void Clear() => throw new NotImplementedException();
    }
}
