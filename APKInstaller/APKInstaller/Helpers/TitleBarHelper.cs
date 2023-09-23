using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System;
using Windows.Win32;
using Windows.Win32.Foundation;
using WinRT.Interop;

namespace APKInstaller.Helpers
{
    public static class TitleBarHelper
    {
        public static void TriggerTitleBarRepaint(Window window)
        {
            // to trigger repaint tracking task id 38044406
            HWND hwnd = new(WindowNative.GetWindowHandle(window));
            IntPtr activeWindow = PInvoke.GetActiveWindow();
            if (hwnd == activeWindow)
            {
                _ = PInvoke.SendMessage(hwnd, PInvoke.WM_ACTIVATE, PInvoke.WA_INACTIVE, IntPtr.Zero);
                _ = PInvoke.SendMessage(hwnd, PInvoke.WM_ACTIVATE, PInvoke.WA_ACTIVE, IntPtr.Zero);
            }
            else
            {
                _ = PInvoke.SendMessage(hwnd, PInvoke.WM_ACTIVATE, PInvoke.WA_ACTIVE, IntPtr.Zero);
                _ = PInvoke.SendMessage(hwnd, PInvoke.WM_ACTIVATE, PInvoke.WA_INACTIVE, IntPtr.Zero);
            }
        }

        public static bool SetTitleBar(Window window, FrameworkElement element)
            => WindowHelper.GetHandle(window, out IntPtr windowHandle) && SetTitleBar(windowHandle, element);

        public static bool SetTitleBar(IntPtr handle, FrameworkElement element)
        {
            void On_PointerReleased(object sender, PointerRoutedEventArgs e)
            {
                _ = PInvoke.ReleaseCapture();
                PointerPoint Point = e.GetCurrentPoint(sender as FrameworkElement);
                switch (Point.Properties.PointerUpdateKind)
                {
                    case PointerUpdateKind.RightButtonReleased:
                        _ = PInvoke.SendMessage(new HWND(handle), PInvoke.WM_NCRBUTTONUP, PInvoke.HTCAPTION, IntPtr.Zero);
                        break;
                }
            }

            void On_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
            {
                _ = PInvoke.ReleaseCapture();
                _ = PInvoke.SendMessage(new HWND(handle), PInvoke.WM_NCLBUTTONDBLCLK, PInvoke.HTCAPTION, IntPtr.Zero);
            }

            element.PointerReleased += On_PointerReleased;
            element.DoubleTapped += On_DoubleTapped;
            return true;
        }
    }
}
