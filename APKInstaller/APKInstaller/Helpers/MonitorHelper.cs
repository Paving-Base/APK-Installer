using AdvancedSharpAdbClient;
using System.Linq;
using System.Net;
using Zeroconf;

namespace APKInstaller.Helpers
{
    public static class MonitorHelper
    {
        private static DeviceMonitor monitor;
        public static DeviceMonitor Monitor
        {
            get
            {
                if (monitor == null && AdbServer.Instance.GetStatus().IsRunning)
                {
                    monitor = new(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
                    monitor.Start();
                }
                return monitor;
            }
        }

        public static ZeroconfResolver.ResolverListener ADBConnectListener { get; } = ZeroconfResolver.CreateListener("_adb-tls-connect._tcp.local.");

        static MonitorHelper()
        {
            ADBConnectListener.ServiceFound += ADBConnectListener_ServiceFound;
        }

        private static async void ADBConnectListener_ServiceFound(object sender, IZeroconfHost e)
        {
            if (AdbServer.Instance.GetStatus().IsRunning)
            {
                await new AdbClient().ConnectAsync(e.IPAddress, e.Services.FirstOrDefault().Value.Port);
            }
        }
    }
}
