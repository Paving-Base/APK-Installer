using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using APKInstaller.Controls;
using APKInstaller.Helpers;
using APKInstaller.Pages.ToolsPages;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace APKInstaller.ViewModels.ToolsPages
{
    public class ProcessesViewModel : INotifyPropertyChanged
    {
        public TitleBar TitleBar;
        public ComboBox DeviceComboBox;
        public List<DeviceData> devices;
        private readonly ProcessesPage _page;

        private List<string> deviceList = new();
        public List<string> DeviceList
        {
            get => deviceList;
            set
            {
                if (deviceList != value)
                {
                    deviceList = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private IEnumerable<AndroidProcess> processes;
        public IEnumerable<AndroidProcess> Processes
        {
            get => processes;
            set
            {
                if (processes != value)
                {
                    processes = value;
                    RaisePropertyChangedEvent();
                }
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

        public async Task GetDevices()
        {
            await Task.Run(async () =>
            {
                _ = (_page?.DispatcherQueue.EnqueueAsync(TitleBar.ShowProgressRing));
                devices = new AdbClient().GetDevices();
                await _page?.DispatcherQueue.EnqueueAsync(DeviceList.Clear);
                if (devices.Count > 0)
                {
                    foreach (DeviceData device in devices)
                    {
                        if (!string.IsNullOrEmpty(device.Name))
                        {
                            await _page?.DispatcherQueue.EnqueueAsync(() => DeviceList.Add(device.Name));
                        }
                        else if (!string.IsNullOrEmpty(device.Model))
                        {
                            await _page?.DispatcherQueue.EnqueueAsync(() => DeviceList.Add(device.Model));
                        }
                        else if (!string.IsNullOrEmpty(device.Product))
                        {
                            await _page?.DispatcherQueue.EnqueueAsync(() => DeviceList.Add(device.Product));
                        }
                        else if (!string.IsNullOrEmpty(device.Serial))
                        {
                            await _page?.DispatcherQueue.EnqueueAsync(() => DeviceList.Add(device.Serial));
                        }
                        else
                        {
                            await _page?.DispatcherQueue.EnqueueAsync(() => DeviceList.Add("Device"));
                        }
                    }
                    await _page?.DispatcherQueue.EnqueueAsync(() =>
                    {
                        DeviceComboBox.ItemsSource = DeviceList;
                        if (DeviceComboBox.SelectedIndex == -1)
                        {
                            DeviceComboBox.SelectedIndex = 0;
                        }
                    });
                }
                else if (Processes != null)
                {
                    await _page?.DispatcherQueue.EnqueueAsync(() => Processes = null);
                }
                _ = (_page?.DispatcherQueue.EnqueueAsync(TitleBar.HideProgressRing));
            });
        }

        public async Task GetProcess()
        {
            await Task.Run(async () =>
            {
                _ = (_page?.DispatcherQueue.EnqueueAsync(TitleBar.ShowProgressRing));
                AdbClient client = new();
                DeviceData device = await _page?.DispatcherQueue.EnqueueAsync(() => { return devices[DeviceComboBox.SelectedIndex]; });
                IEnumerable<AndroidProcess> list = DeviceExtensions.ListProcesses(client, device);
                await _page?.DispatcherQueue.EnqueueAsync(() => Processes = list);
                _ = (_page?.DispatcherQueue.EnqueueAsync(TitleBar.HideProgressRing));
            });
        }
    }

    public class ProcesseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (string)parameter switch
            {
                "Size" => ((double)(int)value).GetSizeString(),
                "Name" => ((string)value).Split('/').Last().Split(':').FirstOrDefault().Split('@').FirstOrDefault(),
                "State" => (AndroidProcessState)value switch
                {
                    AndroidProcessState.Unknown => "Unknown",
                    AndroidProcessState.D => "Sleep(D)",
                    AndroidProcessState.R => "Running",
                    AndroidProcessState.S => "Sleep(S)",
                    AndroidProcessState.T => "Stopped",
                    AndroidProcessState.W => "Paging",
                    AndroidProcessState.X => "Dead",
                    AndroidProcessState.Z => "Defunct",
                    AndroidProcessState.K => "Wakekill",
                    AndroidProcessState.P => "Parked",
                    _ => value.ToString(),
                },
                _ => value.ToString(),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
