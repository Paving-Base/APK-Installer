using AdvancedSharpAdbClient;
using APKInstaller.Controls;
using APKInstaller.Helpers;
using APKInstaller.Models;
using APKInstaller.ViewModels.SettingsPages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Linq;

namespace APKInstaller.Pages.SettingsPages
{
    public sealed partial class PairDevicePage : Page
    {
        internal PairDeviceViewModel Provider;

        public PairDevicePage()
        {
            InitializeComponent();
            Provider = new PairDeviceViewModel(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Provider = new PairDeviceViewModel(this);
            DataContext = Provider;
            Provider.InitializeConnectListener();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Provider.Dispose();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "PairButton":
                    _ = Provider.ConnectWithPairingCode(element.Tag as MDNSDeviceData);
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

        private async void TitleBar_RefreshRequested(TitleBar sender, object args)
        {
            TitleBar.IsRefreshButtonVisible = false;
            TitleBar.ShowProgressRing();
            _ = await MonitorHelper.ConnectPairedDeviceAsync();
            TitleBar.HideProgressRing();
            TitleBar.IsRefreshButtonVisible = true;
        }
    }
}
