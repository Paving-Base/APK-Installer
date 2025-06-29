using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.DeviceCommands.Models;
using AdvancedSharpAdbClient.Models;
using APKInstaller.Controls;
using APKInstaller.Helpers;
using APKInstaller.Pages.ToolsPages;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace APKInstaller.ViewModels.ToolsPages
{
    public partial class ProcessesViewModel : INotifyPropertyChanged
    {
        public TitleBar TitleBar;
        public ComboBox DeviceComboBox;
        public List<DeviceData> devices;
        private readonly ProcessesPage _page;

        public string CachedSortedColumn { get; set; }

        private ObservableCollection<string> deviceList = [];
        public ObservableCollection<string> DeviceList
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

        private ObservableCollection<AndroidProcess> processes = [];
        public ObservableCollection<AndroidProcess> Processes
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

        private async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                if (_page?.DispatcherQueue.HasThreadAccess == false)
                {
                    await _page.DispatcherQueue.ResumeForegroundAsync();
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public ProcessesViewModel(ProcessesPage page) => _page = page;

        public async Task GetDevices()
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();
                devices = [.. (await new AdbClient().GetDevicesAsync()).Where(x => x.State == DeviceState.Online)];
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
                        if (DeviceComboBox.SelectedIndex == -1)
                        {
                            DeviceComboBox.SelectedIndex = 0;
                        }
                    });
                }
                else
                {
                    await _page?.DispatcherQueue.EnqueueAsync(() =>
                    {
                        DeviceComboBox.SelectedIndex = -1;
                        Processes.Clear();
                    });
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ProcessesViewModel)).Error(ex.ExceptionToMessage());
            }
        }

        public async Task SortData(string sortBy, bool ascending)
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();
                CachedSortedColumn = sortBy;
                switch (sortBy)
                {
                    case "Name":
                        Processes = ascending
                            ? new(Processes.OrderBy(item => item.Name.Split('/').Last().Split(':').FirstOrDefault().Split('@').FirstOrDefault()))
                            : new(Processes.OrderByDescending(item => item.Name.Split('/').Last().Split(':').FirstOrDefault().Split('@').FirstOrDefault()));
                        break;
                    case "ProcessId":
                        Processes = ascending
                            ? new(Processes.OrderBy(item => item.ProcessId))
                            : new(Processes.OrderByDescending(item => item.ProcessId));
                        break;
                    case "State":
                        Processes = ascending
                            ? new(Processes.OrderBy(item => item.State))
                            : new(Processes.OrderByDescending(item => item.State));
                        break;
                    case "ResidentSetSize":
                        Processes = ascending
                            ? new(Processes.OrderBy(item => item.ResidentSetSize))
                            : new(Processes.OrderByDescending(item => item.ResidentSetSize));
                        break;
                    case "Detail":
                        Processes = ascending
                            ? new(Processes.OrderBy(item => item.Name))
                            : new(Processes.OrderByDescending(item => item.Name));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ProcessesViewModel)).Error(ex.ExceptionToMessage());
            }
        }

        public async Task GetProcess()
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();
                if (devices != null && devices.Count != 0)
                {
                    _page?.DispatcherQueue.EnqueueAsync(() =>
                    {
                        TitleBar.ShowProgressRing();
                        TitleBar.IsRefreshButtonVisible = false;
                        ProgressHelper.SetState(ProgressState.Indeterminate, true);
                    });
                    AdbClient client = new();
                    int index = await _page?.DispatcherQueue.EnqueueAsync(() => { return DeviceComboBox.SelectedIndex; });
                    IEnumerable<AndroidProcess> list = await client.ListProcessesAsync(devices[index]);
                    Processes = new(list);
                    _page?.DispatcherQueue.EnqueueAsync(() =>
                    {
                        ProgressHelper.SetState(ProgressState.None, true);
                        TitleBar.IsRefreshButtonVisible = true;
                        TitleBar.HideProgressRing();
                    });
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ProcessesViewModel)).Error(ex.ExceptionToMessage());
                _page?.DispatcherQueue.EnqueueAsync(() =>
                {
                    ProgressHelper.SetState(ProgressState.None, true);
                    TitleBar.IsRefreshButtonVisible = true;
                    TitleBar.HideProgressRing();
                });
            }
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
