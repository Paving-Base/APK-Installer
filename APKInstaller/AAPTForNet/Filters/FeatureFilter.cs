using AAPTForNet.Models;
using System.Collections.Generic;

namespace AAPTForNet.Filters
{
    internal class FeatureFilter : BaseFilter
    {
        private readonly List<string> Features = [];

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
