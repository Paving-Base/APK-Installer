using APKInstaller.Helpers;
using APKInstaller.Pages.ToolsPages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Pages.SettingsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TestPage : Page, INotifyPropertyChanged
    {
        internal bool IsExtendsTitleBar
        {
            get => UIHelper.HasTitleBar ? UIHelper.MainWindow.ExtendsContentIntoTitleBar : UIHelper.GetAppWindowForCurrentWindow().TitleBar.ExtendsContentIntoTitleBar;
            set
            {
                if (UIHelper.HasTitleBar)
                {
                    UIHelper.MainWindow.ExtendsContentIntoTitleBar = value;
                }
                else
                {
                    UIHelper.GetAppWindowForCurrentWindow().TitleBar.ExtendsContentIntoTitleBar = value;
                }
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public TestPage() => InitializeComponent();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "OutPIP":
                    UIHelper.MainWindow.GetAppWindowForCurrentWindow().SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.Default);
                    break;
                case "EnterPIP":
                    UIHelper.MainWindow.GetAppWindowForCurrentWindow().SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.CompactOverlay);
                    break;
                case "Processes":
                    _ = Frame.Navigate(typeof(ProcessesPage));
                    break;
                case "Applications":
                    _ = Frame.Navigate(typeof(ApplicationsPage));
                    break;
                default:
                    break;
            }
        }

        private void TitleBar_BackRequested(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}
