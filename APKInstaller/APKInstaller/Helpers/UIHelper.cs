using AdvancedSharpAdbClient;
using APKInstaller.Pages;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Net;
using Windows.UI.Core;

namespace APKInstaller.Helpers
{
    internal static class ADBHelper
    {
        public static DeviceMonitor Monitor = new DeviceMonitor(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdvancedAdbClient.AdbServerPort)));
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

        public static string GetSizeString(this double size)
        {
            int index = 0;
            while (true)
            {
                index++;
                size /= 1024;
                if (size is > 0.7 and < 716.8) { break; }
                else if (size >= 716.8) { continue; }
                else if (size <= 0.7)
                {
                    size *= 1024;
                    index--;
                    break;
                }
            }
            string str = string.Empty;
            switch (index)
            {
                case 0: str = "B"; break;
                case 1: str = "KB"; break;
                case 2: str = "MB"; break;
                case 3: str = "GB"; break;
                case 4: str = "TB"; break;
                default:
                    break;
            }
            return $"{size:N2}{str}";
        }
    }
}
