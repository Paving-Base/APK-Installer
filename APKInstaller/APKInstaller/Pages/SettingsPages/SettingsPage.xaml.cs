using AdvancedSharpAdbClient;
using APKInstaller.Helpers;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Pages.SettingsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
        private IEnumerable<DeviceData> deviceList;
        internal IEnumerable<DeviceData> DeviceList
        {
            get => deviceList;
            set
            {
                deviceList = value;
                RaisePropertyChangedEvent();
                if (!IsOnlyWSA) { ChooseDevice(); }
            }
        }

        private bool isOnlyWSA = SettingsHelper.Get<bool>(SettingsHelper.IsOnlyWSA);
        internal bool IsOnlyWSA
        {
            get => isOnlyWSA;
            set
            {
                SettingsHelper.Set(SettingsHelper.IsOnlyWSA, value);
                isOnlyWSA = SettingsHelper.Get<bool>(SettingsHelper.IsOnlyWSA);
                SelectDeviceBox.SelectionMode = value ? ListViewSelectionMode.None : ListViewSelectionMode.Single;
                if (!value) { ChooseDevice(); }
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        private const string IssuePath = "https://github.com/Paving-Base/APK-Installer/issues";

        internal static string VersionTextBlockText
        {
            get
            {
                string ver = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";
                string name = "APK Installer";
                return $"{name} v{ver}";
            }
        }

        public SettingsPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
#if DEBUG
            GoToTestPage.Visibility = Visibility.Visible;
#endif
            SelectDeviceBox.SelectionMode = IsOnlyWSA ? ListViewSelectionMode.None : ListViewSelectionMode.Single;
            ADBHelper.Monitor.DeviceChanged += OnDeviceChanged;
            DeviceList = new AdvancedAdbClient().GetDevices();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ADBHelper.Monitor.DeviceChanged -= OnDeviceChanged;
        }

        private void OnDeviceChanged(object sender, DeviceDataEventArgs e)
        {
            _ = DispatcherQueue.TryEnqueue(() =>
            {
                DeviceList = new AdvancedAdbClient().GetDevices();
            });
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
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
                case "TestPage":
                    _ = Frame.Navigate(typeof(TestPage));
                    break;
                case "FeedBack":
                    _ = Launcher.LaunchUriAsync(new Uri(IssuePath));
                    break;
                case "LogFolder":
                    _ = await Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists));
                    break;
                default:
                    break;
            }
        }

        private void MarkdownTextBlock_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            _ = Launcher.LaunchUriAsync(new Uri(e.Link));
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
            if (vs != null && vs is DeviceData device)
            {
                SettingsHelper.Set(SettingsHelper.DefaultDevice, device);
            }
        }

        private void ChooseDevice()
        {
            DeviceData device = SettingsHelper.Get<DeviceData>(SettingsHelper.DefaultDevice);
            foreach(DeviceData data in DeviceList)
            {
                if(data.Name == device.Name && data.Model == device.Model && data.Product == device.Product)
                {
                    SelectDeviceBox.SelectedItem = data;
                    break;
                }
            }
        }
    }
}