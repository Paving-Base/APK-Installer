using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;

namespace APKInstaller.Helpers
{
    public enum DwmWindowAttribute : uint
    {
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
        DWMWA_SYSTEMBACKDROP_TYPE = 38,
        DWMWA_MICA_EFFECT = 1029
    }

    public enum BackdropType
    {
        None = 1,
        Mica = 2,
        Acrylic = 3,
        Tabbed = 4
    }

    public static class MicaHelper
    {
        private delegate void NoArgDelegate();

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, ref int pvAttribute, int cbAttribute);
        public static int SetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attribute, int parameter) => DwmSetWindowAttribute(hwnd, attribute, ref parameter, Marshal.SizeOf<int>());

        private static void SetMica(Window window,ElementTheme theme, OSVersion osVersion, BackdropType micaType, int captionHeight)
        {
            int trueValue = 0x01;
            int falseValue = 0x00;

            IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);

            if (UIHelper.IsDarkTheme(theme))
            {
                SetWindowAttribute(windowHandle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, trueValue);
            }
            else
            {
                SetWindowAttribute(windowHandle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, falseValue);
            }

            if (osVersion.Build >= 22523)
            {
                SetWindowAttribute(windowHandle, DwmWindowAttribute.DWMWA_SYSTEMBACKDROP_TYPE, (int)micaType);
            }
            else
            {
                SetWindowAttribute(windowHandle, DwmWindowAttribute.DWMWA_MICA_EFFECT, trueValue);
            }
        }

        public static void EnableMica(this Window window, BackdropType micaType = BackdropType.Mica, int captionHeight = 20)
        {
            OSVersion osVersion = SettingsHelper.OperatingSystemVersion;
            ElementTheme theme = SettingsHelper.Theme;

            SetMica(window, theme, osVersion, micaType, captionHeight);
            SettingsHelper.UISettings.ColorValuesChanged += (s, e) =>
            {
                UIHelper.DispatcherQueue.EnqueueAsync(() =>
                {
                    EnableMica(window, micaType, captionHeight);
                });
            };
        }
    }
}
