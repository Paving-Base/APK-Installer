using System;
using System.Runtime.InteropServices; // For DllImport
using Windows.System;

namespace APKInstaller.Helpers
{
    public class WindowsSystemDispatcherQueueHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct DispatcherQueueOptions
        {
            internal int DWSize;
            internal int ThreadType;
            internal int ApartmentType;
        }

        [DllImport("CoreMessaging.dll")]
        private static extern unsafe int CreateDispatcherQueueController(DispatcherQueueOptions options, IntPtr* instance);

        private IntPtr m_dispatcherQueueController = IntPtr.Zero;
        public void EnsureWindowsSystemDispatcherQueueController()
        {
            if (DispatcherQueue.GetForCurrentThread() != null)
            {
                // one already exists, so we'll just use it.
                return;
            }

            if (m_dispatcherQueueController == IntPtr.Zero)
            {
                DispatcherQueueOptions options;
                options.DWSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
                options.ThreadType = 2;    // DQTYPE_THREAD_CURRENT
                options.ApartmentType = 2; // DQTAT_COM_STA

                unsafe
                {
                    IntPtr dispatcherQueueController;
                    _ = CreateDispatcherQueueController(options, &dispatcherQueueController);
                    m_dispatcherQueueController = dispatcherQueueController;
                }
            }
        }
    }
}
