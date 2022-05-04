using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using APKInstaller.Controls;
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
    public class ApplicationsViewModel : INotifyPropertyChanged
    {
        public TitleBar TitleBar;
        public ComboBox DeviceComboBox;
        public List<DeviceData> devices;
        private readonly ApplicationsPage _page;

        private List<string> deviceList = new();
        public List<string> DeviceList
        {
            get => deviceList;
            set
            {
                deviceList = value;
                RaisePropertyChangedEvent();
            }
        }

        private List<APKInfo> applications;
        public List<APKInfo> Applications
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

        public ApplicationsViewModel(ApplicationsPage page)
        {
            _page = page;
        }

        public async Task GetDevices()
        {
            await Task.Run(async () =>
            {
                _ = (_page?.DispatcherQueue.EnqueueAsync(TitleBar.ShowProgressRing));
                devices = new AdvancedAdbClient().GetDevices();
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
                else if (Applications != null)
                {
                    await _page?.DispatcherQueue.EnqueueAsync(() => Applications = null);
                }
                _ = (_page?.DispatcherQueue.EnqueueAsync(TitleBar.HideProgressRing));
            });
        }

        public async Task<List<APKInfo>> CheckAPP(Dictionary<string, string> apps, int index)
        {
            List<APKInfo> Applications = new();
            await Task.Run(() =>
            {
                AdvancedAdbClient client = new();
                PackageManager manager = new(client, devices[index]);
                foreach (KeyValuePair<string, string> app in apps)
                {
                    _ = _page.DispatcherQueue.EnqueueAsync(() => TitleBar.SetProgressValue((double)apps.ToList().IndexOf(app) * 100 / apps.Count));
                    if (!string.IsNullOrEmpty(app.Key))
                    {
                        ConsoleOutputReceiver receiver = new();
                        client.ExecuteRemoteCommand($"pidof {app.Key}", devices[index], receiver);
                        bool isactive = !string.IsNullOrEmpty(receiver.ToString());
                        Applications.Add(new APKInfo()
                        {
                            Name = app.Key,
                            IsActive = isactive,
                            VersionInfo = manager.GetVersionInfo(app.Key),
                        });
                    }
                }
            });
            return Applications;
        }

        public async Task GetApps()
        {
            await Task.Run(async () =>
            {
                _ = (_page?.DispatcherQueue.EnqueueAsync(TitleBar.ShowProgressRing));
                AdvancedAdbClient client = new();
                int index = await _page?.DispatcherQueue.EnqueueAsync(() => { return DeviceComboBox.SelectedIndex; });
                PackageManager manager = new(new AdvancedAdbClient(), devices[index]);
                List<APKInfo> list = await CheckAPP(manager.Packages, index);
                await _page?.DispatcherQueue.EnqueueAsync(() => Applications = list);
                _ = (_page?.DispatcherQueue.EnqueueAsync(TitleBar.HideProgressRing));
            });
        }
    }

    internal class ApplicationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (string)parameter switch
            {
                "State" => (bool)value ? "Running" : "Stop",
                _ => value.ToString(),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => (Visibility)value == Visibility.Visible;
    }

    public class APKInfo
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public VersionInfo VersionInfo { get; set; }
    }
}
