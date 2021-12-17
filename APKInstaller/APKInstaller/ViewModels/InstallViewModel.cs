using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using APKInstaller.Pages;
using PortableDownloader;
using SharpCompress.Common;
using SharpCompress.Archives;
using System.Diagnostics;
using AdvancedSharpAdbClient;
using System.Net;
using APKInstaller.Helpers;
using Microsoft.UI.Dispatching;
using CommunityToolkit.WinUI;

namespace APKInstaller.ViewModels
{
    public class InstallViewModel : INotifyPropertyChanged
    {
        private InstallPage _page;
        private DeviceData _device;
#if !DEBUG
        private string _path;
#else
        private string _path = @"C:\Users\qq251\Downloads\Programs\Minecraft_1.17.40.06_sign.apk";
#endif
        private new readonly DispatcherQueue DispatcherQueue;
        private static bool IsOnlyWSA => SettingsHelper.Get<bool>(SettingsHelper.IsOnlyWSA);
        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("InstallPage");

        private bool isInstalling;
        internal bool IsInstalling
        {
            get => isInstalling;
            set
            {
                isInstalling = value;
                RaisePropertyChangedEvent();
            }
        }

        private string _waitProgressText;
        public string WaitProgressText
        {
            get => _waitProgressText;
            set
            {
                _waitProgressText = value;
                RaisePropertyChangedEvent();
            }
        }

        private double _waitProgressValue;
        public double WaitProgressValue
        {
            get => _waitProgressValue;
            set
            {
                _waitProgressValue = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        private async Task CheckADB(bool force = false)
        {
            if (!force && File.Exists(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"platform-tools\adb.exe")))
            {
                WaitProgressText = _loader.GetString("ADBExist");
            }
            else
            {
                StackPanel StackPanel = new StackPanel();
                StackPanel.Children.Add(
                    new TextBlock()
                    {
                        TextWrapping = TextWrapping.Wrap,
                        Text = _loader.GetString("AboutADB")
                    });
                StackPanel.Children.Add(
                    new HyperlinkButton()
                    {
                        Content = _loader.GetString("ClickToRead"),
                        NavigateUri = new Uri("https://developer.android.google.cn/studio/releases/platform-tools?hl=zh-cn")
                    });
                ContentDialog dialog = new ContentDialog()
                {
                    XamlRoot = _page.XamlRoot,
                    Title = _loader.GetString("ADBMissing"),
                    PrimaryButtonText = _loader.GetString("OK"),
                    CloseButtonText = _loader.GetString("Cancel"),
                    Content = new ScrollViewer()
                    {
                        Content = StackPanel
                    },
                    DefaultButton = ContentDialogButton.Primary
                };
                ContentDialogResult result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await DownloadADB();
                }
                else
                {
                    Application.Current.Exit();
                }
            }
        }

        private async Task DownloadADB()
        {
            using Downloader downloader = new Downloader(new DownloaderOptions()
            {
                Uri = new Uri("https://dl.google.com/android/repository/platform-tools-latest-windows.zip?hl=zh-cn"),
                Stream = File.OpenWrite(Path.Combine(ApplicationData.Current.LocalFolder.Path, "platform-tools.zip"))
            });
            _ = downloader.Start();
            while (downloader.TotalSize <= 0 && downloader.IsStarted)
            {
                WaitProgressText = _loader.GetString("WaitDownload");
                await Task.Delay(500);
            }
            while (downloader.IsStarted)
            {
                WaitProgressText = $"{((double)downloader.BytesPerSecond).GetSizeString()}/s";
                WaitProgressValue = (double)downloader.CurrentSize * 100 / downloader.TotalSize;
                await Task.Delay(500);
            }
            WaitProgressText = _loader.GetString("UnzipADB");
            WaitProgressValue = 0;
            IArchive archive = ArchiveFactory.Open(Path.Combine(ApplicationData.Current.LocalFolder.Path, "platform-tools.zip"));
            foreach (IArchiveEntry entry in archive.Entries)
            {
                WaitProgressValue = archive.Entries.GetProgressValue(entry);
                WaitProgressText = string.Format(_loader.GetString("UnzippingFormat"), archive.Entries.ToList().IndexOf(entry) + 1, archive.Entries.Count());
                if (!entry.IsDirectory)
                {
                    entry.WriteToDirectory(ApplicationData.Current.LocalFolder.Path, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                }
            }
            WaitProgressValue = 0;
            WaitProgressText = _loader.GetString("UnzipComplete");
        }

        private async Task InitilizeADB()
        {
            WaitProgressText = _loader.GetString("CheckingADB");
            await CheckADB();
            if (!string.IsNullOrEmpty(_path))
            {
                WaitProgressText = _loader.GetString("StartingADB");
                Process[] processes = Process.GetProcessesByName("adb");
                if (processes != null && processes.Length > 1)
                {
                    foreach (Process process in processes)
                    {
                        process.Kill();
                    }
                }
                if (processes != null && processes.Length == 1)
                {
                    try
                    {
                        await Task.Run(() => new AdbServer().StartServer(processes.First().MainModule.FileName, restartServerIfNewer: false));
                    }
                    catch
                    {
                        foreach (Process process in processes)
                        {
                            process.Kill();
                        }
                        try
                        {
                            await Task.Run(() => new AdbServer().StartServer(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"platform-tools\adb.exe"), restartServerIfNewer: false));
                        }
                        catch
                        {
                            await CheckADB(true);
                            WaitProgressText = _loader.GetString("StartingADB");
                            await Task.Run(() => new AdbServer().StartServer(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"platform-tools\adb.exe"), restartServerIfNewer: false));
                        }
                    }
                }
                else
                {
                    try
                    {
                        await Task.Run(() => new AdbServer().StartServer(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"platform-tools\adb.exe"), restartServerIfNewer: false));
                    }
                    catch
                    {
                        await CheckADB(true);
                        WaitProgressText = _loader.GetString("StartingADB");
                        await Task.Run(() => new AdbServer().StartServer(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"platform-tools\adb.exe"), restartServerIfNewer: false));
                    }
                }
                if (IsOnlyWSA)
                {
                    new AdvancedAdbClient().Connect(new DnsEndPoint("127.0.0.1", 58526));
                }
                ADBHelper.Monitor.DeviceChanged += OnDeviceChanged;
            }
        }

        private void OnDeviceChanged(object sender, DeviceDataEventArgs e)
        {
            if (IsOnlyWSA)
            {
                new AdvancedAdbClient().Connect(new DnsEndPoint("127.0.0.1", 58526));
            }
            if (!IsInstalling)
            {
                DispatcherQueue.EnqueueAsync(() =>
                {
                    if (CheckDevice() && _device != null)
                    {
                        //CheckAPK();
                    }
                });
            }
        }

        private bool CheckDevice()
        {
            AdvancedAdbClient client = new AdvancedAdbClient();
            List<DeviceData> devices = client.GetDevices();
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            if (devices.Count <= 0) { return false; }
            foreach (DeviceData device in devices)
            {
                if (device == null || device.State == DeviceState.Offline) { continue; }
                if (IsOnlyWSA)
                {
                    client.ExecuteRemoteCommand("getprop ro.boot.hardware", device, receiver);
                    if (receiver.ToString().Contains("windows"))
                    {
                        _device = device ?? _device;
                        return true;
                    }
                }
                else
                {
                    DeviceData data = SettingsHelper.Get<DeviceData>(SettingsHelper.DefaultDevice);
                    if (data != null && data.Name == device.Name && data.Model == device.Model && data.Product == device.Product)
                    {
                        _device = data;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}