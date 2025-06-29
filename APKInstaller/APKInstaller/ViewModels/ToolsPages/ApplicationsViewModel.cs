using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.DeviceCommands.Models;
using AdvancedSharpAdbClient.Models;
using AdvancedSharpAdbClient.Receivers;
using APKInstaller.Controls;
using APKInstaller.Helpers;
using APKInstaller.Pages.ToolsPages;
using CommunityToolkit.WinUI;
using IWshRuntimeLibrary;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using File = System.IO.File;

namespace APKInstaller.ViewModels.ToolsPages
{
    public partial class ApplicationsViewModel : INotifyPropertyChanged
    {
        public TitleBar TitleBar;
        public ComboBox DeviceComboBox;
        public List<DeviceData> devices;
        private readonly ApplicationsPage _page;
        private Dictionary<string, (string Name, BitmapImage Icon)> PackageInfos;

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

        private ObservableCollection<APKInfo> applications = [];
        public ObservableCollection<APKInfo> Applications
        {
            get => applications;
            set
            {
                if (applications != value)
                {
                    applications = value;
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

        public ApplicationsViewModel(ApplicationsPage page) => _page = page;

        private async Task GetInfos()
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();
                string ProgramFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
                string[] InkInfos = Directory.GetFiles(ProgramFolder, "*.lnk");
                PackageInfos = [];
                foreach (string file in InkInfos)
                {
                    WshShell shell = new();
                    WshShortcut shortcut = shell.CreateShortcut(file);
                    string args = shortcut.Arguments;
                    string icon = shortcut.IconLocation.Replace(",0", string.Empty);
                    string path = shortcut.TargetPath;
                    if (Path.GetFileNameWithoutExtension(path) == "WsaClient")
                    {
                        string pic = Path.ChangeExtension(icon, "png");
                        if (File.Exists(pic)) { icon = pic; }
                        Uri imageUri = new(pic);
                        BitmapImage image = await UIHelper.DispatcherQueue.EnqueueAsync(() => { return new BitmapImage(imageUri); });
                        PackageInfos.Add(args.Replace("/launch wsa://", string.Empty), (Path.GetFileNameWithoutExtension(file), image));
                    }
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ApplicationsViewModel)).Error(ex.ExceptionToMessage());
            }
        }

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
                        Applications.Clear();
                    });
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ApplicationsViewModel)).Error(ex.ExceptionToMessage());
            }
        }

        public async Task CheckAPP(Dictionary<string, string> apps, int index)
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();
                await UIHelper.DispatcherQueue.EnqueueAsync(Applications.Clear);
                AdbClient client = new();
                PackageManager manager = new(client, devices[index]);
                if (PackageInfos == null) { await GetInfos(); }
                ProgressHelper.SetState(ProgressState.Normal, true);
                foreach (KeyValuePair<string, string> app in apps)
                {
                    ProgressHelper.SetValue(apps.ToList().IndexOf(app), apps.Count, true);
                    TitleBar.SetProgressValue((double)apps.ToList().IndexOf(app) * 100 / apps.Count);
                    if (!string.IsNullOrEmpty(app.Key))
                    {
                        ConsoleOutputReceiver receiver = new();
                        await client.ExecuteRemoteCommandAsync($"pidof {app.Key}", devices[index], receiver);
                        bool isActive = !string.IsNullOrEmpty(receiver.ToString());
                        if (PackageInfos.TryGetValue(app.Key, out (string Name, BitmapImage Icon) value))
                        {
                            (string Name, BitmapImage Icon) = value;
                            await UIHelper.DispatcherQueue.EnqueueAsync(async () =>
                            {
                                ImageIcon source = new() { Source = Icon, Width = 20, Height = 20 };
                                Applications.Add(new APKInfo
                                {
                                    Name = Name,
                                    Icon = source,
                                    PackageName = app.Key,
                                    IsActive = isActive,
                                    VersionInfo = await manager.GetVersionInfoAsync(app.Key),
                                });
                            });
                        }
                        else
                        {
                            await UIHelper.DispatcherQueue.EnqueueAsync(async () =>
                            {
                                FontIcon source = new() { Glyph = "\xECAA" };
                                Applications.Add(new APKInfo
                                {
                                    Name = app.Key,
                                    Icon = source,
                                    IsActive = isActive,
                                    VersionInfo = await manager.GetVersionInfoAsync(app.Key),
                                });
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ApplicationsViewModel)).Error(ex.ExceptionToMessage());
            }
        }

        public async Task GetApps()
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
                    PackageManager manager = new(client, devices[index]);
                    await CheckAPP(manager.Packages, index);
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
                SettingsHelper.LogManager.GetLogger(nameof(ApplicationsViewModel)).Error(ex.ExceptionToMessage());
                _page?.DispatcherQueue.EnqueueAsync(() =>
                {
                    ProgressHelper.SetState(ProgressState.None, true);
                    TitleBar.IsRefreshButtonVisible = true;
                    TitleBar.HideProgressRing();
                });
            }
        }
    }

    internal partial class ApplicationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (string)parameter switch
            {
                "State" => (bool)value ? "Running" : "Stop",
                _ => value.ToString(),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }

    public class APKInfo
    {
        public string Name { get; set; }
        public IconElement Icon { get; set; }
        public string PackageName { get; set; }
        public bool IsActive { get; set; }
        public VersionInfo VersionInfo { get; set; }
    }
}
