using AAPTForNet.Models;
using System.Collections.Generic;

namespace AAPTForNet.Filters
{
    internal class PermissionFilter : BaseFilter
    {
        private readonly List<string> Permissions = [];

        public override bool CanHandle(string msg) => msg.StartsWith("uses-permission:");

        public override void AddMessage(string msg)
        {
            // uses-permission: name='<per>'
            // -> ["uses-permission: name=", "<per, get this value!!!>", ""]
            Permissions.Add(msg.Split(Seperator)[1]);
        }

        public override ApkInfo GetAPK() => new() { Permissions = Permissions };

        public override void Clear() => Permissions.Clear();
    }
}
