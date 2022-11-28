using AAPTForNet.Models;
using AdvancedSharpAdbClient;
using APKInstaller.Pages;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using WinRT.Interop;

namespace APKInstaller.Helpers
{
    public static class ADBHelper
    {
        public static DeviceMonitor Monitor = new(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));

        static ADBHelper()
        {
            Monitor.Start();
        }
    }

    public static partial class UIHelper
    {
        public static bool HasTitleBar = !AppWindowTitleBar.IsCustomizationSupported();
        public static bool TitleBarExtended => HasTitleBar ? MainWindow.ExtendsContentIntoTitleBar : WindowHelper.GetAppWindowForCurrentWindow().TitleBar.ExtendsContentIntoTitleBar;
        public static double TitleBarHeight => TitleBarExtended ? HasTitleBar ? 28 : 32 : 0;
        public static double PageTitlePadding => TitleBarHeight;

        private static DispatcherQueue _dispatcherQueue;
        public static DispatcherQueue DispatcherQueue
        {
            get => _dispatcherQueue;
            set
            {
                if (_dispatcherQueue == null)
                {
                    _dispatcherQueue = value;
                }
            }
        }
    }

    public static partial class UIHelper
    {
        public static MainPage MainPage;
        public static MainWindow MainWindow;

        public static void Navigate(Type pageType, NavigationTransitionInfo TransitionInfo, object e = null)
        {
            _ = (DispatcherQueue?.EnqueueAsync(() => { _ = (MainPage?.CoreAppFrame.Navigate(pageType, e, TransitionInfo)); }));
        }

        public static int GetActualPixel(this double pixel)
        {
            IntPtr windowHandle = WindowNative.GetWindowHandle(MainWindow);
            int currentDpi = PInvoke.User32.GetDpiForWindow(windowHandle);
            return Convert.ToInt32(pixel * (currentDpi / 96.0));
        }
    }

    public static partial class UIHelper
    {
        public static string GetSizeString(this double size)
        {
            int index = 0;
            while (index <= 11)
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
                case 5: str = "PB"; break;
                case 6: str = "EB"; break;
                case 7: str = "ZB"; break;
                case 8: str = "YB"; break;
                case 9: str = "BB"; break;
                case 10: str = "NB"; break;
                case 11: str = "DB"; break;
                default:
                    break;
            }
            return $"{size:0.##}{str}";
        }

        public static string GetPermissionName(this string permission)
        {
            ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("Permissions");
            try
            {
                string name = _loader.GetString(permission) ?? string.Empty;
                return string.IsNullOrWhiteSpace(name) ? permission : name;
            }
            catch (Exception e)
            {
                SettingsHelper.LogManager.GetLogger(nameof(UIHelper)).Warn(e.ExceptionToMessage(), e);
                return permission;
            }
        }

        public static double GetProgressValue<T>(this IList<T> lists, T list)
        {
            return (double)(lists.IndexOf(list) + 1) * 100 / lists.Count;
        }

        public static double GetProgressValue<T>(this IEnumerable<T> lists, T list)
        {
            return (double)(lists.ToList().IndexOf(list) + 1) * 100 / lists.Count();
        }

        public static Uri ValidateAndGetUri(this string uriString)
        {
            Uri uri = null;
            try
            {
                uri = new Uri(uriString.Contains("://") ? uriString : uriString.Contains("//") ? uriString.Replace("//", "://") : $"http://{uriString}");
            }
            catch (FormatException e)
            {
                SettingsHelper.LogManager.GetLogger(nameof(UIHelper)).Error(e.ExceptionToMessage(), e);
            }
            return uri;
        }

        public static string ExceptionToMessage(this Exception ex)
        {
            StringBuilder builder = new();
            builder.Append('\n');
            if (!string.IsNullOrWhiteSpace(ex.Message)) { builder.AppendLine($"Message: {ex.Message}"); }
            builder.AppendLine($"HResult: {ex.HResult} (0x{Convert.ToString(ex.HResult, 16)})");
            if (!string.IsNullOrWhiteSpace(ex.StackTrace)) { builder.AppendLine(ex.StackTrace); }
            if (!string.IsNullOrWhiteSpace(ex.HelpLink)) { builder.Append($"HelperLink: {ex.HelpLink}"); }
            return builder.ToString();
        }

        public static Color ColorMixing(Color c1, Color c2)
        {
            double a1 = c1.A / 255;
            double a2 = c2.A / 255;
            int a = Math.Min(c1.A + c2.A, 255);
            int r = Convert.ToInt32(Math.Min((c1.R * a1) + (c2.R * a2), 255));
            int g = Convert.ToInt32(Math.Min((c1.G * a1) + (c2.G * a2), 255));
            int b = Convert.ToInt32(Math.Min((c1.B * a1) + (c2.B * a2), 255));
            Color color_mixing = Color.FromArgb(Convert.ToByte(a), Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
            return color_mixing;
        }
    }
}
