using AAPTForNet.Models;
using APKInstaller.Controls;
using APKInstaller.Helpers;
using APKInstaller.ViewModels.AboutPages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using Windows.Storage;
using Windows.System;

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

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "SharePackage":
                    DataTransferHelper.ShareFile(Provider.ApkInfo.PackagePath, Provider.ApkInfo.AppName);
                    break;
                case "OpenPackageFolder":
                    _ = await Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(Provider.ApkInfo.PackagePath[..Provider.ApkInfo.PackagePath.LastIndexOf('\\')]));
                    break;
                default:
                    break;
            }
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
