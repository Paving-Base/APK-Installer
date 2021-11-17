using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using APKInstaller.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Pages.ToolsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProcessesPage : Page, INotifyPropertyChanged
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

        private IEnumerable<AndroidProcess> processes;
        internal IEnumerable<AndroidProcess> Processes
        {
            get => processes;
            set
            {
                processes = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public ProcessesPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ADBHelper.Monitor.DeviceChanged += OnDeviceChanged;
            await Task.Run(() => _ = DispatcherQueue.TryEnqueue(() => GetDevices()));
        }

        private void GetDevices()
        {
            TitleBar.ShowProgressRing();
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
            else if (Processes != null)
            {
                Processes = null;
            }
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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AdvancedAdbClient client = new AdvancedAdbClient();
            Processes = DeviceExtensions.ListProcesses(client, devices[(sender as ComboBox).SelectedIndex]);
        }

        private void TitleBar_RefreshEvent(object sender, RoutedEventArgs e)
        {
            TitleBar.ShowProgressRing();
            GetDevices();
            TitleBar.ShowProgressRing();
            AdvancedAdbClient client = new AdvancedAdbClient();
            Processes = DeviceExtensions.ListProcesses(client, devices[DeviceComboBox.SelectedIndex]);
            TitleBar.HideProgressRing();
        }
    }

    internal class ProcesseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch ((string)parameter)
            {
                case "Size": return ((double)(int)value).GetSizeString();
                case "Name": return ((string)value).Split('/').Last().Split(':').First().Split('@').First();
                case "State":
                    switch ((AndroidProcessState)value)
                    {
                        case AndroidProcessState.Unknown: return "Unknown";
                        case AndroidProcessState.D: return "Sleep(D)";
                        case AndroidProcessState.R: return "Running";
                        case AndroidProcessState.S: return "Sleep(S)";
                        case AndroidProcessState.T: return "Stopped";
                        case AndroidProcessState.W: return "Paging";
                        case AndroidProcessState.X: return "Dead";
                        case AndroidProcessState.Z: return "Defunct";
                        case AndroidProcessState.K: return "Wakekill";
                        case AndroidProcessState.P: return "Parked";
                        default: return value.ToString();
                    }
                default: return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => (Visibility)value == Visibility.Visible;
    }
}
