using AdvancedSharpAdbClient;
using APKInstaller.Helpers;
using APKInstaller.Pages;
using Microsoft.UI.Xaml;
using PInvoke;
using System;
using System.Diagnostics;
using Windows.Graphics;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public BackdropHelper Backdrop;

        public MainWindow()
        {
            InitializeComponent();
            this.GetAppWindowForCurrentWindow().SetIcon("favicon.ico");
            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            Backdrop = new BackdropHelper(this);
            SetWindowSize(hwnd, 652, 414);
            UIHelper.MainWindow = this;
            MainPage MainPage = new();
            Content = MainPage;
            SetBackdrop();
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            Process[] processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            if (processes.Length <= 1)
            {
                CachesHelper.CleanAllCaches(true);

                if (SettingsHelper.Get<bool>(SettingsHelper.IsCloseADB))
                {
                    try { new AdbClient().KillAdb(); }
                    catch (Exception e) { SettingsHelper.LogManager.GetLogger(nameof(MainWindow)).Error(e.ExceptionToMessage(), e); }
                }
            }
            else
            {
                CachesHelper.CleanAllCaches(false);
            }
        }

        private void SetWindowSize(IntPtr hwnd, int width, int height)
        {
            int dpi = User32.GetDpiForWindow(hwnd);
            float scalingFactor = (float)dpi / 96;
            width = (int)(width * scalingFactor);
            height = (int)(height * scalingFactor);
            this.GetAppWindowForCurrentWindow().Resize(new SizeInt32(width, height));
        }

        private void SetBackdrop()
        {
            BackdropType type = SettingsHelper.Get<BackdropType>(SettingsHelper.SelectedBackdrop);
            Backdrop.SetBackdrop(type);
        }
    }
}
