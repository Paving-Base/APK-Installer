using AdvancedSharpAdbClient;
using APKInstaller.Pages;
using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using WinRT.Interop;
using WindowId = Microsoft.UI.WindowId;

namespace APKInstaller.Helpers
{
    internal static class ADBHelper
    {
        public static DeviceMonitor Monitor = new(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdvancedAdbClient.AdbServerPort)));
        static ADBHelper()
        {
            Monitor.Start();
        }
    }

    internal static partial class UIHelper
    {
        public static bool HasTitleBar = !AppWindowTitleBar.IsCustomizationSupported();
        public static double TitleBarHeight => HasTitleBar ? 28 : 32;
        public static double PageTitlePadding => HasTitleBar ? 0 : TitleBarHeight;

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

        public static bool IsDarkTheme(ElementTheme theme)
        {
            return theme == ElementTheme.Default ? Application.Current.RequestedTheme == ApplicationTheme.Dark : theme == ElementTheme.Dark;
        }

        public static bool IsDarkTheme() => IsDarkTheme(SettingsHelper.Theme);

        public static void CheckTheme()
        {
            if (!HasTitleBar)
            {
                AppWindowTitleBar TitleBar = GetAppWindowForCurrentWindow().TitleBar;

                ResourceDictionary ResourceDictionary = new ResourceDictionary();
                ResourceDictionary.Source = new Uri("ms-appx:///Controls/TitleBar/TitleBar_themeresources.xaml");

                // rest colors
                var buttonForegroundColor = (Color)ResourceDictionary["TitleBarButtonForegroundColor"];
                TitleBar.ButtonForegroundColor = buttonForegroundColor;

                var buttonBackgroundColor = (Color)ResourceDictionary["TitleBarButtonBackgroundColor"];
                TitleBar.ButtonBackgroundColor = buttonBackgroundColor;
                TitleBar.ButtonInactiveBackgroundColor = buttonBackgroundColor;

                // hover colors
                var buttonHoverForegroundColor = (Color)ResourceDictionary["TitleBarButtonHoverForegroundColor"];
                TitleBar.ButtonHoverForegroundColor = buttonHoverForegroundColor;

                var buttonHoverBackgroundColor = (Color)ResourceDictionary["TitleBarButtonHoverBackgroundColor"];
                TitleBar.ButtonHoverBackgroundColor = buttonHoverBackgroundColor;

                // pressed colors
                var buttonPressedForegroundColor = (Color)ResourceDictionary["TitleBarButtonPressedForegroundColor"];
                TitleBar.ButtonPressedForegroundColor = buttonPressedForegroundColor;

                var buttonPressedBackgroundColor = (Color)ResourceDictionary["TitleBarButtonPressedBackgroundColor"];
                TitleBar.ButtonPressedBackgroundColor = buttonPressedBackgroundColor;

                // inactive foreground
                var buttonInactiveForegroundColor = (Color)ResourceDictionary["TitleBarButtonInactiveForegroundColor"];
                TitleBar.ButtonInactiveForegroundColor = buttonInactiveForegroundColor;

                TitleBar.BackgroundColor = (Color)Application.Current.Resources["SolidBackgroundFillColorBase"];
            }
        }
    }

    internal static partial class UIHelper
    {
        public static MainPage MainPage;
        public static MainWindow MainWindow;

        public static void Navigate(Type pageType, NavigationTransitionInfo TransitionInfo, object e = null)
        {
            DispatcherQueue?.EnqueueAsync(() =>
            {
                _ = (MainPage?.CoreAppFrame.Navigate(pageType, e, TransitionInfo));
            });
        }

        public static int GetActualPixel(this double pixel)
        {
            IntPtr windowHandle = WindowNative.GetWindowHandle(MainWindow);
            int currentDpi = PInvoke.User32.GetDpiForWindow(windowHandle);
            return Convert.ToInt32(pixel * (currentDpi / 96.0));
        }

        public static AppWindow GetAppWindowForCurrentWindow(this Window window)
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(window);
            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(myWndId);
        }

        public static AppWindow GetAppWindowForCurrentWindow()
        {
            return MainWindow != null ? GetAppWindowForCurrentWindow(MainWindow) : null;
        }
    }

    internal static partial class UIHelper
    {
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

        public static string GetPermissionName(this string permission)
        {
            ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("Permissions");
            try
            {
                string name = _loader.GetString(permission) ?? string.Empty;
                return string.IsNullOrEmpty(name) ? permission : name;
            }
            catch
            {
                return permission;
            }
        }

        public static double GetProgressValue<T>(this List<T> lists, T list)
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
            catch (FormatException)
            {

            }
            return uri;
        }
    }
}
