using APKInstaller.Helpers;
using APKInstaller.Pages.SettingsPages;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool HasBeenSmail;

        public MainPage()
        {
            InitializeComponent();
            UIHelper.MainPage = this;
            if (UIHelper.HasTitleBar)
            {
                UIHelper.MainWindow.ExtendsContentIntoTitleBar = true;
            }
            else
            {
                CustomTitleBar.HorizontalAlignment = HorizontalAlignment.Stretch;
                AppWindow AppWindow = UIHelper.GetAppWindowForCurrentWindow();
                AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                UIHelper.CheckTheme();
            }
            UIHelper.MainWindow.SetTitleBar(CustomTitleBar);
            _ = CoreAppFrame.Navigate(typeof(InstallPage));
        }

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

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (UIHelper.HasTitleBar)
                {
                    if (XamlRoot.Size.Width <= 240)
                    {
                        if (!HasBeenSmail)
                        {
                            HasBeenSmail = true;
                            UIHelper.MainWindow.SetTitleBar(null);
                        }
                    }
                    else if (HasBeenSmail)
                    {
                        HasBeenSmail = false;
                        UIHelper.MainWindow.SetTitleBar(CustomTitleBar);
                    }
                    CustomTitleBar.Width = XamlRoot.Size.Width - 120;
                }
            }
            catch { }
        }
    }
}
