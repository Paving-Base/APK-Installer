using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using APKInstaller.Controls;
using APKInstaller.Helpers;
using APKInstaller.Pages.ToolsPages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace APKInstaller.ViewModels.ToolsPages
{
    public class ProcessesViewModel : INotifyPropertyChanged
    {
        public TitleBar TitleBar;
        public ComboBox DeviceComboBox;
        public List<DeviceData> devices;
        private readonly ProcessesPage _page;

        private List<string> deviceList = new List<string>();
        public List<string> DeviceList
        {
            get => deviceList;
            set
            {
                deviceList = value;
                RaisePropertyChangedEvent();
            }
        }

        private IEnumerable<AndroidProcess> processes;
        public IEnumerable<AndroidProcess> Processes
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

        public ProcessesViewModel(ProcessesPage page)
        {
            _page = page;
        }

        public void GetDevices()
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
                DeviceComboBox.ItemsSource = DeviceList;
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
    }

    public class ProcesseConverter : IValueConverter
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
