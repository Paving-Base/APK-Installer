using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using NativeMethods.Interop;
using System;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace APKInstaller.Helpers
{
    public static class TitleBarHelper
    {
        public const int WA_ACTIVE = 0x01;
        public const int WA_INACTIVE = 0x00;

        public static void TriggerTitleBarRepaint()
        {
            // to trigger repaint tracking task id 38044406
            IntPtr hwnd = WindowNative.GetWindowHandle(UIHelper.MainWindow);
            IntPtr activeWindow = PInvoke.User32.GetActiveWindow();
            if (hwnd == activeWindow)
            {
                _ = User32.SendMessage(hwnd, User32.WM.ACTIVATE, (IntPtr)WA_INACTIVE, IntPtr.Zero);
                _ = User32.SendMessage(hwnd, User32.WM.ACTIVATE, (IntPtr)WA_ACTIVE, IntPtr.Zero);
            }
            else
            {
                _ = User32.SendMessage(hwnd, User32.WM.ACTIVATE, (IntPtr)WA_ACTIVE, IntPtr.Zero);
                _ = User32.SendMessage(hwnd, User32.WM.ACTIVATE, (IntPtr)WA_INACTIVE, IntPtr.Zero);
            }

        }

        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public static bool SetTitleBar(Window window, FrameworkElement element)
            => WindowHelper.GetHandle(window, out IntPtr windowHandle) && SetTitleBar(windowHandle, element);

        public static bool SetTitleBar(IntPtr handle, FrameworkElement element)
        {
            void On_PointerReleased(object sender, PointerRoutedEventArgs e)
            {
                ReleaseCapture();
                PointerPoint Point = e.GetCurrentPoint(sender as FrameworkElement);
                switch (Point.Properties.PointerUpdateKind)
                {
                    case PointerUpdateKind.RightButtonReleased:
                        _ = User32.SendMessage(handle, User32.WM.NCRBUTTONUP, (IntPtr)User32.WM_NCHITTEST.CAPTION, (IntPtr)0);
                        break;
                }
            }

            void On_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
            {
                ReleaseCapture();
                _ = User32.SendMessage(handle, User32.WM.NCLBUTTONDBLCLK, (IntPtr)User32.WM_NCHITTEST.CAPTION, (IntPtr)0);
            }

            element.PointerReleased += On_PointerReleased;
            element.DoubleTapped += On_DoubleTapped;
            return true;
        }

    }
}
