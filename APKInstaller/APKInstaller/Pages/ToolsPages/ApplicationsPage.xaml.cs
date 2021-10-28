using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using APKInstaller.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Pages.ToolsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ApplicationsPage : Page, INotifyPropertyChanged
    {
        private List<DeviceData> devices;

        private List<string> deviceList = new List<string>();
        internal List<string> DeviceList
        {
            get => deviceList;
            set
            {
                deviceList = value;
                RaisePropertyChangedEvent();
            }
        }

        private List<APKInfo> applications;
        internal List<APKInfo> Applications
        {
            get => applications;
            set
            {
                applications = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public ApplicationsPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await Task.Run(() => _ = DispatcherQueue.TryEnqueue(() =>
            {
                ADBHelper.Monitor.DeviceChanged += OnDeviceChanged;
                GetDevices();
            }));
        }

        private void GetDevices()
        {
            devices = new AdvancedAdbClient().GetDevices();
            DeviceList.Clear();
            if (devices.Count > 0)
            {
                foreach (DeviceData device in devices)
                {
                    if (!string.IsNullOrEmpty(device.Name))
                    {
                        DeviceList.Add(device.Name);
                    }
                    else if (!string.IsNullOrEmpty(device.Model))
                    {
                        DeviceList.Add(device.Model);
                    }
                    else if (!string.IsNullOrEmpty(device.Product))
                    {
                        DeviceList.Add(device.Product);
                    }
                    else if (!string.IsNullOrEmpty(device.Serial))
                    {
                        DeviceList.Add(device.Serial);
                    }
                    else
                    {
                        DeviceList.Add("Device");
                    }
                }
                if (DeviceComboBox.SelectedIndex == -1)
                {
                    DeviceComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                Applications.Clear();
            }
        }

        private List<APKInfo> CheckAPP(Dictionary<string, string> apps, int index)
        {
            List<APKInfo> Applications = new List<APKInfo>();
            AdvancedAdbClient client = new AdvancedAdbClient();
            PackageManager manager = new PackageManager(client, devices[index]);
            foreach (KeyValuePair<string, string> app in apps)
            {
                _ = DispatcherQueue.TryEnqueue(() => TitleBar.SetProgressValue((double)apps.ToList().IndexOf(app) * 100 / apps.Count));
                if (!string.IsNullOrEmpty(app.Key))
                {
                    ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
                    client.ExecuteRemoteCommand($"pidof {app.Key}", devices[index], receiver);
                    bool isactive = !string.IsNullOrEmpty(receiver.ToString());
                    Applications.Add(new APKInfo()
                    {
                        Name = app.Key,
                        IsActive = isactive,
                        VersionInfo = manager.GetVersionInfo(app.Key)
                    });
                }
            }
            return Applications;
        }

        private async Task Refresh()
        {
            TitleBar.ShowProgressRing();
            GetDevices();
            int index = DeviceComboBox.SelectedIndex;
            PackageManager manager = new PackageManager(new AdvancedAdbClient(), devices[DeviceComboBox.SelectedIndex]);
            Applications = await Task.Run(()=> { return CheckAPP(manager.Packages, index); });
            TitleBar.HideProgressRing();
        }

        private void OnDeviceChanged(object sender, DeviceDataEventArgs e) => _ = DispatcherQueue.TryEnqueue(() => GetDevices());

        private void TitleBar_BackRequested(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TitleBar.ShowProgressRing();
            int index = DeviceComboBox.SelectedIndex;
            PackageManager manager = new PackageManager(new AdvancedAdbClient(), devices[DeviceComboBox.SelectedIndex]);
            Applications = await Task.Run(() => { return CheckAPP(manager.Packages, index); });
            TitleBar.HideProgressRing();
        }

        private async void TitleBar_RefreshEvent(object sender, RoutedEventArgs e) => await Refresh();

        private void DataGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (ApplicationDataGrid.SelectedIndex != -1)
            {
                string Text = (ApplicationDataGrid.SelectedItem as APKInfo).IsActive ? "Stop" : "Start";
                Actions.Tag = Text;
                Actions.Text = Text;
                MenuFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            switch((sender as FrameworkElement).Tag)
            {
                case "Stop":
                    new AdvancedAdbClient().StopApp(devices[DeviceComboBox.SelectedIndex], (ApplicationDataGrid.SelectedItem as APKInfo).Name);
                    break;
                case "Start":
                    new AdvancedAdbClient().StartApp(devices[DeviceComboBox.SelectedIndex], (ApplicationDataGrid.SelectedItem as APKInfo).Name);
                    break;
                case "Uninstall":
                    break;
            }
        }
    }

    internal class ApplicationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch ((string)parameter)
            {
                case "State":return (bool)value ? "Running" : "Stop";
                default: return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => (Visibility)value == Visibility.Visible;
    }

    internal class APKInfo
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public VersionInfo VersionInfo { get; set; }
    }
}
