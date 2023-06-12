using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using System;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using WinRT.Interop;

namespace APKInstaller.Helpers
{
    /// <summary>
    /// Allows to change the status of the displayed notification in the application icon on the TaskBar.
    /// <para>
    /// Based on the work of <see href="https://wpf.2000things.com/2014/03/19/1032-show-progress-on-windows-taskbar-icon/">Sean Sexton</see>.
    /// </para>
    /// </summary>
    public static class ProgressHelper
    {
        private static readonly Window CurrentApplicationWindow;

        private static readonly ITaskbarList4 _taskbarList;

        static ProgressHelper()
        {
            if (!IsSupported()) { return; }

            CurrentApplicationWindow = UIHelper.MainWindow;

            _taskbarList = new TaskbarList() as ITaskbarList4;

            _taskbarList?.HrInit();
        }

        private static bool IsSupported() => OSVersionHelper.IsWindows7OrGreater;

        /// <summary>
        /// Allows to change the status of the progress bar in the task bar.
        /// </summary>
        /// <param name="state">State of the progress indicator.</param>
        /// <param name="dispatchInvoke">Run with the main <see cref="Application"/> thread.</param>
        public static async void SetState(ProgressState state, bool dispatchInvoke = false)
        {
            if (!(dispatchInvoke || CurrentApplicationWindow.DispatcherQueue.HasThreadAccess))
            {
                await CurrentApplicationWindow.DispatcherQueue.ResumeForegroundAsync();
            }
            SetProgressState(state);
        }

        /// <summary>
        /// Allows to change the fill of the task bar.
        /// </summary>
        /// <param name="current">Current value to display</param>
        /// <param name="max">Maximum number for division.</param>
        /// <param name="dispatchInvoke">Run with the main <see cref="Application"/> thread.</param>
        public static async void SetValue(int current, int max, bool dispatchInvoke = false)
        {
            if (!(dispatchInvoke || CurrentApplicationWindow.DispatcherQueue.HasThreadAccess))
            {
                await CurrentApplicationWindow.DispatcherQueue.ResumeForegroundAsync();
            }
            SetProgressValue(current, max);
        }

        private static void SetProgressState(ProgressState state)
        {
            _taskbarList?.SetProgressState(GetHandle(), (TBPFLAG)(int)state);
        }

        private static void SetProgressValue(int current, int max)
        {
            _taskbarList?.SetProgressValue(
                GetHandle(),
                Convert.ToUInt64(current),
                Convert.ToUInt64(max));
        }

        private static HWND GetHandle() =>
            new(CurrentApplicationWindow != null ? WindowNative.GetWindowHandle(CurrentApplicationWindow) : IntPtr.Zero);
    }

    /// <summary>
    /// Specifies the state of the progress indicator in the Windows taskbar.
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.windows.shell.taskbaritemprogressstate?view=windowsdesktop-5.0"/>
    /// </summary>
    public enum ProgressState
    {
        /// <summary>
        /// No progress indicator is displayed in the taskbar button.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// A pulsing green indicator is displayed in the taskbar button.
        /// </summary>
        Indeterminate = 0x1,

        /// <summary>
        /// A green progress indicator is displayed in the taskbar button.
        /// </summary>
        Normal = 0x2,

        /// <summary>
        /// A red progress indicator is displayed in the taskbar button.
        /// </summary>
        Error = 0x4,

        /// <summary>
        /// A yellow progress indicator is displayed in the taskbar button.
        /// </summary>
        Paused = 0x8
    }
}
