using APKInstaller.Helpers;
using APKInstaller.Pages.SettingsPages;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public readonly string GetAppTitleFromSystem = ResourceLoader.GetForViewIndependentUse()?.GetString("AppName") ?? Package.Current.DisplayName;

        public MainPage()
        {
            InitializeComponent();
            UIHelper.MainPage = this;
            UIHelper.DispatcherQueue = DispatcherQueue.GetForCurrentThread();
            UIHelper.MainWindow.Backdrop.BackdropTypeChanged += OnBackdropTypeChanged;
            if (UIHelper.HasTitleBar)
            {
                UIHelper.MainWindow.ExtendsContentIntoTitleBar = true;
            }
            else
            {
                UIHelper.MainWindow.AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                ActualThemeChanged += (sender, arg) => ThemeHelper.UpdateSystemCaptionButtonColors();
            }
            UIHelper.MainWindow.SetTitleBar(CustomTitleBar);
            _ = CoreAppFrame.Navigate(typeof(InstallPage));
        }

        private void OnBackdropTypeChanged(BackdropHelper sender, object args) => RootBackground.Opacity = (BackdropType)args == BackdropType.DefaultColor ? 1 : 0;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Name)
            {
                case "AboutButton":
                    _ = CoreAppFrame.Navigate(typeof(SettingsPage));
                    break;
                default:
                    break;
            }
        }

        private void CustomTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!UIHelper.HasTitleBar)
            {
                RectInt32 Rect = new((ActualWidth - CustomTitleBar.ActualWidth).GetActualPixel(), 0, CustomTitleBar.ActualWidth.GetActualPixel(), CustomTitleBar.ActualHeight.GetActualPixel());
                UIHelper.MainWindow.AppWindow.TitleBar.SetDragRectangles([Rect]);
            }
        }

        internal void UpdateTitleBarHeight() => CustomTitleBar.Height = UIHelper.TitleBarHeight;
    }
}
