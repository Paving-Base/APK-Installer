using AAPTForNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAPTForNet.Filters
{
    internal class FeatureFilter : BaseFilter
    {
        private readonly List<string> Features = new();

        public override bool CanHandle(string msg) => msg.StartsWith("  uses-feature");

        public override void AddMessage(string msg)
        {
            // uses-feature: name='<per>'
            // -> ["uses-feature: name=", "<per, get this value!!!>", ""]
            Features.Add(msg.Split(Seperator)[1]);
        }

        public override ApkInfo GetAPK() => new() { Features = Features };

        public override void Clear() => Features.Clear();

    }
}
