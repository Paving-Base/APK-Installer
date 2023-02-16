using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using File = System.IO.File;

namespace APKInstaller.ViewModels.ToolsPages
{
    public class ApplicationsViewModel : INotifyPropertyChanged
    {
        public TitleBar TitleBar;
        public ComboBox DeviceComboBox;
        public List<DeviceData> devices;
        private readonly ApplicationsPage _page;
        private Dictionary<string, (string Name, BitmapImage Icon)> PackageInfos;

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

        private List<APKInfo> applications;
        public List<APKInfo> Applications
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

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public ApplicationsViewModel(ApplicationsPage page)
        {
            _page = page;
        }

        private async Task GetInfos()
        {
            await Task.Run(async () =>
            {
                string ProgramFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
                string[] InkInfos = Directory.GetFiles(ProgramFolder, "*.lnk");
                PackageInfos = new Dictionary<string, (string Name, BitmapImage Icon)>();
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
                        Uri imageuri = new(pic);
                        BitmapImage image = await UIHelper.DispatcherQueue.EnqueueAsync(() => { return new BitmapImage(imageuri); });
                        PackageInfos.Add(args.Replace("/launch wsa://", string.Empty), (Path.GetFileNameWithoutExtension(file), image));
                    }
                }
            });
        }

        public async Task GetDevices()
        {
            await Task.Run(async () =>
            {
                ProgressHelper.SetState(ProgressState.Indeterminate, true);
                _ = (_page?.DispatcherQueue.EnqueueAsync(TitleBar.ShowProgressRing));
                devices = new AdbClient().GetDevices().Where(x => x.State == DeviceState.Online).ToList();
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
                ProgressHelper.SetState(ProgressState.None, true);
            });
        }

        public async Task<List<APKInfo>> CheckAPP(Dictionary<string, string> apps, int index)
        {
            List<APKInfo> Applications = new();
            await Task.Run(async () =>
            {
                AdbClient client = new();
                PackageManager manager = new(client, devices[index]);
                if (PackageInfos == null) { await GetInfos(); }
                ProgressHelper.SetState(ProgressState.Normal, true);
                foreach (KeyValuePair<string, string> app in apps)
                {
                    ProgressHelper.SetValue(apps.ToList().IndexOf(app), apps.Count, true);
                    _ = _page.DispatcherQueue.EnqueueAsync(() => TitleBar.SetProgressValue((double)apps.ToList().IndexOf(app) * 100 / apps.Count));
                    if (!string.IsNullOrEmpty(app.Key))
                    {
                        ConsoleOutputReceiver receiver = new();
                        client.ExecuteRemoteCommand($"pidof {app.Key}", devices[index], receiver);
                        bool isactive = !string.IsNullOrEmpty(receiver.ToString());
                        if (PackageInfos.ContainsKey(app.Key))
                        {
                            (string Name, BitmapImage Icon) = PackageInfos[app.Key];
                            ImageIcon source = await UIHelper.DispatcherQueue.EnqueueAsync(() => { return new ImageIcon { Source = Icon, Width = 20, Height = 20 }; });
                            Applications.Add(new APKInfo
                            {
                                Name = Name,
                                Icon = source,
                                PackageName = app.Key,
                                IsActive = isactive,
                                VersionInfo = manager.GetVersionInfo(app.Key),
                            });
                        }
                        else
                        {
                            FontIcon source = await UIHelper.DispatcherQueue.EnqueueAsync(() => { return new FontIcon { Glyph = "\xECAA" }; });
                            Applications.Add(new APKInfo
                            {
                                Name = app.Key,
                                Icon = source,
                                IsActive = isactive,
                                VersionInfo = manager.GetVersionInfo(app.Key),
                            });
                        }
                    }
                }
            });
            return Applications;
        }

        public async Task GetApps()
        {
            await Task.Run(async () =>
            {
                ProgressHelper.SetState(ProgressState.Indeterminate, true);
                _ = (_page?.DispatcherQueue.EnqueueAsync(TitleBar.ShowProgressRing));
                AdbClient client = new();
                int index = await _page?.DispatcherQueue.EnqueueAsync(() => { return DeviceComboBox.SelectedIndex; });
                PackageManager manager = new(new AdbClient(), devices[index]);
                List<APKInfo> list = await CheckAPP(manager.Packages, index);
                await _page?.DispatcherQueue.EnqueueAsync(() => Applications = list);
                _ = (_page?.DispatcherQueue.EnqueueAsync(TitleBar.HideProgressRing));
                ProgressHelper.SetState(ProgressState.None, true);
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
