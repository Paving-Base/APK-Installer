using AdvancedSharpAdbClient;
using APKInstaller.Helpers;
using APKInstaller.Pages;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, ref int pvAttribute, int cbAttribute);

        public MainWindow()
        {
            InitializeComponent();
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            UIHelper.MainWindow = this;
            MainPage MainPage = new();
            EnableMica(hwnd, true);
            Content = MainPage;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            if (SettingsHelper.Get<bool>(SettingsHelper.IsCloseADB))
            {
                new AdvancedAdbClient().KillAdb();
            }
        }

        public enum DwmWindowAttribute : uint
        {
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_MICA_EFFECT = 1029
        }
        public static void EnableMica(IntPtr source, bool darkThemeEnabled)
        {
            int trueValue = 0x01;
            int falseValue = 0x00;

            // Set dark mode before applying the material, otherwise you'll get an ugly flash when displaying the window.
            if (darkThemeEnabled)
            {
                _ = DwmSetWindowAttribute(source, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref trueValue, Marshal.SizeOf(typeof(int)));
            }
            else
            {
                _ = DwmSetWindowAttribute(source, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref falseValue, Marshal.SizeOf(typeof(int)));
            }

            _ = DwmSetWindowAttribute(source, DwmWindowAttribute.DWMWA_MICA_EFFECT, ref trueValue, Marshal.SizeOf(typeof(int)));
        }
    }
}
