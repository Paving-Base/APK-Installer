using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using WinRT.Interop;

namespace APKInstaller.Helpers
{
    /// <summary>
    /// Helpers class to allow the app to find the Window that contains an
    /// arbitrary <see cref="UIElement"/> (<see cref="GetWindowForElement(UIElement)"/>).
    /// To do this, we keep track of all active Windows. The app code must call
    /// <see cref="CreateWindow()"/> rather than "new <see cref="Window"/>()"
    /// so we can keep track of all the relevant windows.
    /// </summary>
    public static class WindowHelper
    {
        public static Window CreateWindow()
        {
            Window newWindow = new();
            TrackWindow(newWindow);
            return newWindow;
        }

        public static void TrackWindow(this Window window)
        {
            window.Closed += (sender, args) =>
            {
                ActiveWindows.Remove(window);
            };
            ActiveWindows.Add(window);
        }

        public static Window GetWindowForElement(this UIElement element)
        {
            if (element.XamlRoot != null)
            {
                foreach (Window window in ActiveWindows)
                {
                    if (element.XamlRoot == window.Content.XamlRoot)
                    {
                        return window;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Tries to get the pointer to the window handle.
        /// </summary>
        /// <returns><see langword="true"/> if the handle is not <see cref="IntPtr.Zero"/>.</returns>
        public static bool GetHandle(Window window, out IntPtr windowHandle)
        {
            windowHandle = WindowNative.GetWindowHandle(window);
            return windowHandle != IntPtr.Zero;
        }

        public static HashSet<Window> ActiveWindows { get; } = new HashSet<Window>();
    }
}
