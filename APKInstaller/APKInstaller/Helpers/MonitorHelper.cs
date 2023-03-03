using AdvancedSharpAdbClient;
using Zeroconf;
using Zeroconf.Interfaces;
using System.Linq;
using System.Net;

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
                    _monitor.Start();
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
                ConnectListener.ServiceFound += ADBConnectListener_ServiceFound;
            }
        }

        public static void DisposeConnectListener()
        {
            if (ConnectListener != null)
            {
                ConnectListener.ServiceFound -= ADBConnectListener_ServiceFound;
                ConnectListener.Dispose();
            }
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
