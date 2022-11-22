using AAPTForNet.Models;
using APKInstaller.Controls;
using APKInstaller.ViewModels.AboutPages;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Pages.AboutPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InfosPage : Page
    {
        internal InfosViewModel Provider;

        public InfosPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is ApkInfo info)
            {
                Provider = new InfosViewModel(info, this);
            }
            DataContext = Provider;
        }

        private void TitleBar_BackRequested(TitleBar sender, object e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}
