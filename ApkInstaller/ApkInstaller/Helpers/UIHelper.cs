using APKInstaller.Pages;
using Microsoft.UI.Xaml.Media.Animation;
using SharpAdbClient;
using System;
using System.Net;
using Windows.UI.Core;

namespace APKInstaller.Helpers
{
    internal static class ADBHelper
    {
        public static DeviceMonitor Monitor = new DeviceMonitor(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
        static ADBHelper()
        {
            Monitor.Start();
        }
    }

    internal static partial class UIHelper
    {
        public static MainPage MainPage;
        public static MainWindow MainWindow;

        public static void Navigate(Type pageType, NavigationTransitionInfo TransitionInfo, object e = null)
        {
            _ = (MainPage?.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _ = (MainPage?.CoreAppFrame.Navigate(pageType, e, TransitionInfo));
            }));
        }
    }
}
