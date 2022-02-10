using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AAPTForNet.Helpers
{
    internal static class Utils
    {
        public static string GetMD5(this string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] r1 = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                string r2 = BitConverter.ToString(r1).ToLowerInvariant();
                return r2.Replace("-", "");
            }
        }
    }
}
