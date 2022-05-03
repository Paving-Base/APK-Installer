using AdvancedSharpAdbClient;
using APKInstaller.Controls;
using APKInstaller.Helpers;
using APKInstaller.Models;
using APKInstaller.ViewModels.SettingsPages;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using Windows.Storage;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Pages.SettingsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        internal SettingsViewModel Provider;

        public SettingsPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (SettingsViewModel.Caches != null)
            {
                Provider = SettingsViewModel.Caches;
            }
            else
            {
                Provider = new SettingsViewModel(this);
                if (Provider.UpdateDate == DateTime.MinValue) { Provider.CheckUpdate(); }
                if (new AdbServer().GetStatus().IsRunning)
                {
                    ADBHelper.Monitor.DeviceChanged += Provider.OnDeviceChanged;
                    Provider.DeviceList = new AdvancedAdbClient().GetDevices();
                }
            }
            DataContext = Provider;
            //#if DEBUG
            GoToTestPage.Visibility = Visibility.Visible;
            //#endif
            SelectDeviceBox.SelectionMode = Provider.IsOnlyWSA ? ListViewSelectionMode.None : ListViewSelectionMode.Single;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (new AdbServer().GetStatus().IsRunning) { ADBHelper.Monitor.DeviceChanged -= Provider.OnDeviceChanged; }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "Rate":
                    _ = Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9P2JFQ43FPPG"));
                    break;
                case "Group":
                    _ = Launcher.LaunchUriAsync(new Uri("https://t.me/PavingBase"));
                    break;
                case "Reset":
                    ApplicationData.Current.LocalSettings.Values.Clear();
                    SettingsHelper.SetDefaultSettings();
                    if (Reset.Flyout is Flyout flyout_reset)
                    {
                        flyout_reset.Hide();
                    }
                    _ = Frame.Navigate(typeof(SettingsPage));
                    Frame.GoBack();
                    break;
                case "ADBPath":
                    Provider.ChangeADBPath();
                    break;
                case "Connect":
                    new AdvancedAdbClient().Connect(ConnectIP.Text);
                    Provider.OnDeviceChanged(null, null);
                    break;
                case "TestPage":
                    _ = Frame.Navigate(typeof(TestPage));
                    break;
                case "LogFolder":
                    _ = await Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists));
                    break;
                case "CheckUpdate":
                    Provider.CheckUpdate();
                    break;
                default:
                    break;
            }
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "ADBPath":
                    _ = await Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(Provider.ADBPath[..Provider.ADBPath.LastIndexOf(@"\")]));
                    break;
                case "LogFolder":
                    _ = await Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists));
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

        private void SelectDeviceBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object vs = (sender as ListView).SelectedItem;
            if (vs is not null and DeviceData device)
            {
                SettingsHelper.Set(SettingsHelper.DefaultDevice, device);
            }
        }

        private void GotoUpdate_Click(object sender, RoutedEventArgs e) => _ = Launcher.LaunchUriAsync(new Uri((sender as FrameworkElement).Tag.ToString()));

        private void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e) => _ = Launcher.LaunchUriAsync(new Uri(e.Link));

        private void WebXAML_Loaded(object sender, RoutedEventArgs e) => (sender as WebXAML).ContentInfo = new GitInfo("Paving-Base", "APK-Installer", "screenshots", "Documents/Announcements", "Announcements.xaml");
    }
}