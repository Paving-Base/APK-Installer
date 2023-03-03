using AdvancedSharpAdbClient;
using Zeroconf;
using Zeroconf.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace APKInstaller.Helpers
{
    public static class AddressHelper
    {
        public static async Task<List<string>> GetAddressID(string mac)
        {
            List<string> addresses = new();
            Regex Regex = new($@"\s*(\d+.\d+.\d+.\d+)\s*{mac}\S*\s*\w+");
            List<string> lines = await CommandHelper.ExecuteShellCommandAsync($"arp -a|findstr {mac}");
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
            AdbClient AdbClient = new();
            List<string> addresses = await GetAddressID("00-15-5d");
            foreach (string address in addresses)
            {
                _ = AdbClient.ConnectAsync(address);
            }
        }

        public static async Task<List<string>> ConnectHyperVAsync()
        {
            AdbClient AdbClient = new();
            List<string> addresses = await GetAddressID("00-15-5d");
            List<string> results = new();
            foreach (string address in addresses)
            {
                results.Add(await AdbClient.ConnectAsync(address));
            }
            return results;
        }

        public static async Task ConnectPairedDevice()
        {
            IReadOnlyList<IZeroconfHost> hosts = await ZeroconfResolver.ResolveAsync("_adb-tls-connect._tcp.local.");
            if (hosts.Any())
            {
                AdbClient AdbClient = new();
                foreach (IZeroconfHost host in hosts)
                {
                    _ = AdbClient.ConnectAsync(host.IPAddress, host.Services.FirstOrDefault().Value.Port);
                }
            }
        }

        public static async Task<List<string>> ConnectPairedDeviceAsync()
        {
            List<string> results = new();
            IReadOnlyList<IZeroconfHost> hosts = await ZeroconfResolver.ResolveAsync("_adb-tls-connect._tcp.local.");
            if (hosts.Any())
            {
                AdbClient AdbClient = new();
                foreach (IZeroconfHost host in hosts)
                {
                    results.Add(await AdbClient.ConnectAsync(host.IPAddress, host.Services.FirstOrDefault().Value.Port));
                }
            }
            return results;
        }
    }
}
