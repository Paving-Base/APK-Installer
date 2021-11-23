using AAPTForNet.Models;
using System.Collections.Generic;

namespace AAPTForNet.Filters
{
    internal class PermissionFilter : BaseFilter
    {
        private readonly List<string> permissions = new List<string>();

        public override bool canHandle(string msg)
        {
            return msg.StartsWith("uses-permission:");
        }

        public override void addMessage(string msg)
        {
            // uses-permission: name='<per>'
            // -> ["uses-permission: name=", "<per, get this value!!!>", ""]
            permissions.Add(msg.Split(seperator)[1]);
        }

        public override ApkInfo getAPK()
        {
            return new ApkInfo()
            {
                Permissions = permissions
            };
        }

        public override void clear() => permissions.Clear();
    }
}
