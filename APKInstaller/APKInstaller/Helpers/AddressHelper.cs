using AdvancedSharpAdbClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace APKInstaller.Helpers
{
    public static class AddressHelper
    {
        public static async Task<List<string>> GetAddressID(string mac)
        {
            List<string> addresses = new List<string>();
            Regex Regex = new Regex($@"\s*(\d+.\d+.\d+.\d+)\s*{mac}\S*\s*\w+");
            List<string> lines = await CommandHelper.ExecuteShellCommand($"arp -a|findstr {mac}");
            foreach (string line in lines)
            {
                if (Regex.IsMatch(line))
                {
                    addresses.Add(Regex.Match(line).Groups[1].Value);
                }
            }
            return addresses;
        }

        public static async Task ConnectHyperV()
        {
            AdvancedAdbClient AdbClient = new AdvancedAdbClient();
            List<string> addresses = await GetAddressID("00-15-5d");
            foreach( string address in addresses)
            {
                AdbClient.Connect(address);
            }
        }
    }
}
