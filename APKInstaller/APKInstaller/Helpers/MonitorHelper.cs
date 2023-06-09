using AdvancedSharpAdbClient;
using Zeroconf;
using Zeroconf.Interfaces;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace APKInstaller.Helpers
{
    public static class MonitorHelper
    {
        private static DeviceMonitor _monitor;
        public static DeviceMonitor Monitor
        {
            get
            {
                if (_monitor == null && AdbServer.Instance.GetStatus().IsRunning)
                {
                    _monitor = new(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
                    _ = _monitor.StartAsync();
                }
                return _monitor;
            }
        }

        public static ResolverListener ConnectListener { get; private set; }

        public static void InitializeConnectListener()
        {
            if (ConnectListener == null)
            {
                ConnectListener = ZeroconfResolver.CreateListener("_adb-tls-connect._tcp.local.");
                ConnectListener.ServiceFound += ConnectListener_ServiceFound;
            }
        }

        public static void DisposeConnectListener()
        {
            if (ConnectListener != null)
            {
                ConnectListener.ServiceFound -= ConnectListener_ServiceFound;
                ConnectListener.Dispose();
            }
        }

        private static async void ConnectListener_ServiceFound(object sender, IZeroconfHost e)
        {
            if ((await AdbServer.Instance.GetStatusAsync(CancellationToken.None)).IsRunning)
            {
                await new AdbClient().ConnectAsync(e.IPAddress, e.Services.FirstOrDefault().Value.Port);
            }
        }

        public static async Task ConnectPairedDevice()
        {
            IReadOnlyList<IZeroconfHost> hosts = ConnectListener != null
                ? ConnectListener.Hosts
                : await ZeroconfResolver.ResolveAsync("_adb-tls-connect._tcp.local.");
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
            IReadOnlyList<IZeroconfHost> hosts = ConnectListener != null
                ? ConnectListener.Hosts
                : await ZeroconfResolver.ResolveAsync("_adb-tls-connect._tcp.local.");
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
