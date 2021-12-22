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
using AAPTForNet.Models;
using AAPTForNet;
using APKInstaller.Controls.Dialogs;
using AdvancedSharpAdbClient.DeviceCommands;

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

        private ApkInfo _apkInfo = null;
        public ApkInfo ApkInfo
        {
            get => _apkInfo;
            set
            {
                _apkInfo = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _isOpenApp = SettingsHelper.Get<bool>(SettingsHelper.IsOpenApp);
        public bool IsOpenApp
        {
            get => _isOpenApp;
            set
            {
                SettingsHelper.Set(SettingsHelper.IsOpenApp, value);
                _isOpenApp = SettingsHelper.Get<bool>(SettingsHelper.IsOpenApp);
                RaisePropertyChangedEvent();
            }
        }

        private bool _isInstalling;
        public bool IsInstalling
        {
            get => _isInstalling;
            set
            {
                _isInstalling = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _isInitialized;
        public bool IsInitialized
        {
            get => _isInitialized;
            set
            {
                _isInitialized = value;
                RaisePropertyChangedEvent();
            }
        }

        private string _appName;
        public string AppName
        {
            get => _appName;
            set
            {
                _appName = value;
                RaisePropertyChangedEvent();
            }
        }

        private string _textOutput;
        public string TextOutput
        {
            get => _textOutput;
            set
            {
                _textOutput = value;
                RaisePropertyChangedEvent();
            }
        }

        private string _infoMessage;
        public string InfoMessage
        {
            get => _infoMessage;
            set
            {
                _infoMessage = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _actionButtonEnable;
        public bool ActionButtonEnable
        {
            get => _actionButtonEnable;
            set
            {
                _actionButtonEnable = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _secondaryActionButtonEnable;
        public bool SecondaryActionButtonEnable
        {
            get => _secondaryActionButtonEnable;
            set
            {
                _secondaryActionButtonEnable = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _cancelOperationButtonEnable;
        public bool CancelOperationButtonEnable
        {
            get => _cancelOperationButtonEnable;
            set
            {
                _cancelOperationButtonEnable = value;
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

        private string _actionButtonText;
        public string ActionButtonText
        {
            get => _actionButtonText;
            set
            {
                _actionButtonText = value;
                RaisePropertyChangedEvent();
            }
        }

        private string _secondaryActionButtonText;
        public string SecondaryActionButtonText
        {
            get => _secondaryActionButtonText;
            set
            {
                _secondaryActionButtonText = value;
                RaisePropertyChangedEvent();
            }
        }

        private string _cancelOperationButtonText;
        public string CancelOperationButtonText
        {
            get => _cancelOperationButtonText;
            set
            {
                _cancelOperationButtonText = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _textOutputVisibility;
        public Visibility TextOutputVisibility
        {
            get => _textOutputVisibility;
            set
            {
                _textOutputVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _installOutputVisibility;
        public Visibility InstallOutputVisibility
        {
            get => _installOutputVisibility;
            set
            {
                _installOutputVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _actionVisibility;
        public Visibility ActionVisibility
        {
            get => _actionVisibility;
            set
            {
                _actionVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _secondaryActionVisibility;
        public Visibility SecondaryActionVisibility
        {
            get => _secondaryActionVisibility;
            set
            {
                _secondaryActionVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _cancelOperationVisibility;
        public Visibility CancelOperationVisibility
        {
            get => _cancelOperationVisibility;
            set
            {
                _cancelOperationVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _messagesToUserVisibility;
        public Visibility MessagesToUserVisibility
        {
            get => _messagesToUserVisibility;
            set
            {
                _messagesToUserVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _launchWhenReadyVisibility;
        public Visibility LaunchWhenReadyVisibility
        {
            get => _launchWhenReadyVisibility;
            set
            {
                _launchWhenReadyVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _appVersionVisibility;
        public Visibility AppVersionVisibility
        {
            get => _appVersionVisibility;
            set
            {
                _appVersionVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _appPublisherVisibility;
        public Visibility AppPublisherVisibility
        {
            get => _appPublisherVisibility;
            set
            {
                _appPublisherVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _appCapabilitiesVisibility;
        public Visibility AppCapabilitiesVisibility
        {
            get => _appCapabilitiesVisibility;
            set
            {
                _appCapabilitiesVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public async Task CheckADB(bool force = false)
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

        public async Task DownloadADB()
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

        public async Task InitilizeADB()
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

        public async Task InitilizeUI()
        {
            if (!string.IsNullOrEmpty(_path))
            {
                WaitProgressText = _loader.GetString("Loading");
                try
                {
                    ApkInfo = await Task.Run(() => { return AAPTool.Decompile(_path); });
                }
                catch (Exception ex)
                {
                    PackageError(ex.Message);
                    IsInitialized = true;
                    return;
                }
                if (string.IsNullOrEmpty(ApkInfo.PackageName))
                {
                    PackageError(_loader.GetString("InvalidPackage"));
                }
                else
                {
                    WaitProgressText = _loader.GetString("Checking");
                    if (CheckDevice() && _device != null)
                    {
                        CheckAPK();
                    }
                    else
                    {
                        ResetUI();
                        ActionButtonEnable = false;
                        ActionButtonText = _loader.GetString("Install");
                        InfoMessage = _loader.GetString("WaitingDevice");
                        ActionVisibility = MessagesToUserVisibility = Visibility.Visible;
                        AppName = string.Format(_loader.GetString("WaitingForInstallFormat"), ApkInfo.AppName);
                        if (IsOnlyWSA)
                        {
                            ContentDialog dialog = new MarkdownDialog()
                            {
                                XamlRoot = _page.XamlRoot,
                                CloseButtonText = _loader.GetString("IKnow"),
                                Title = _loader.GetString("HowToConnect"),
                                DefaultButton = ContentDialogButton.Close,
                                ContentUrl = "https://raw.githubusercontent.com/Paving-Base/APK-Installer/screenshots/Helpers/How%20To%20Connect%20WSA/How%20To%20Connect%20WSA.md",
                            };
                            _ = dialog.ShowAsync();
                        }
                    }
                }
                WaitProgressText = _loader.GetString("Finished");
            }
            else
            {
                ResetUI();
                ApkInfo = new ApkInfo();
                AppName = _loader.GetString("NoPackageWranning");
                AppVersionVisibility = AppPublisherVisibility = AppCapabilitiesVisibility = Visibility.Collapsed;
            }
            IsInitialized = true;
        }

        public void CheckAPK()
        {
            ResetUI();
            AdvancedAdbClient client = new AdvancedAdbClient();
            if (_device == null)
            {
                ActionButtonEnable = false;
                ActionButtonText = _loader.GetString("Install");
                InfoMessage = _loader.GetString("WaitingDevice");
                ActionVisibility = MessagesToUserVisibility = Visibility.Visible;
                AppName = string.Format(_loader.GetString("WaitingForInstallFormat"), ApkInfo.AppName);
                ContentDialog dialog = new MarkdownDialog()
                {
                    XamlRoot = _page.XamlRoot,
                    ContentUrl = "https://raw.githubusercontent.com/Paving-Base/APK-Installer/screenshots/Helpers/How%20To%20Connect%20WSA/How%20To%20Connect%20WSA.md",
                };
                _ = dialog.ShowAsync();
                return;
            }
            PackageManager manager = new PackageManager(client, _device);
            VersionInfo info = null;
            if (ApkInfo != null)
            {
                info = manager.GetVersionInfo(ApkInfo.PackageName);
            }
            if (info == null)
            {
                ActionButtonText = _loader.GetString("Install");
                AppName = string.Format(_loader.GetString("InstallFormat"), ApkInfo.AppName);
                ActionVisibility = LaunchWhenReadyVisibility = Visibility.Visible;
            }
            else if (info.VersionCode < int.Parse(ApkInfo.VersionCode))
            {
                ActionButtonText = _loader.GetString("Update");
                AppName = string.Format(_loader.GetString("UpdateFormat"), ApkInfo.AppName);
                ActionVisibility = LaunchWhenReadyVisibility = Visibility.Visible;
            }
            else
            {
                ActionButtonText = _loader.GetString("Reinstall");
                SecondaryActionButtonText = _loader.GetString("Launch");
                AppName = string.Format(_loader.GetString("ReinstallFormat"), ApkInfo.AppName);
                TextOutput = string.Format(_loader.GetString("ReinstallOutput"), ApkInfo.AppName);
                ActionVisibility = SecondaryActionVisibility = TextOutputVisibility = Visibility.Visible;
            }
        }

        private void ResetUI()
        {
            ActionVisibility =
            SecondaryActionVisibility =
            CancelOperationVisibility =
            TextOutputVisibility =
            InstallOutputVisibility =
            LaunchWhenReadyVisibility =
            MessagesToUserVisibility = Visibility.Collapsed;
            ActionButtonEnable =
            SecondaryActionButtonEnable =
            CancelOperationButtonEnable = true;
        }

        private void PackageError(string message)
        {
            ResetUI();
            ApkInfo = new ApkInfo();
            TextOutput = message;
            AppName = _loader.GetString("CannotOpenPackage");
            TextOutputVisibility = InstallOutputVisibility = Visibility.Visible;
            AppVersionVisibility = AppPublisherVisibility = AppCapabilitiesVisibility = Visibility.Collapsed;
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

        public bool CheckDevice()
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

        public void OpenAPP() => new AdvancedAdbClient().StartApp(_device, ApkInfo.PackageName);

        public async void InstallAPP()
        {
            try
            {
                IsInstalling = true;
                CancelOperationButtonText = _loader.GetString("Cancel");
                CancelOperationVisibility = LaunchWhenReadyVisibility = Visibility.Visible;
                ActionVisibility = SecondaryActionVisibility = TextOutputVisibility = InstallOutputVisibility = Visibility.Collapsed;
                await Task.Run(() =>
                {
                    new AdvancedAdbClient().Install(_device, File.Open(_path, FileMode.Open, FileAccess.Read));
                });
                if (IsOpenApp)
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1000);// 据说如果安装完直接启动会崩溃。。。
                        OpenAPP();
                    });
                }
                IsInstalling = false;
                SecondaryActionVisibility = Visibility.Visible;
                SecondaryActionButtonText = _loader.GetString("Launch");
                CancelOperationVisibility = LaunchWhenReadyVisibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                IsInstalling = false;
                TextOutput = ex.Message;
                TextOutputVisibility = InstallOutputVisibility = Visibility.Visible;
                ActionVisibility = SecondaryActionVisibility = CancelOperationVisibility = LaunchWhenReadyVisibility = Visibility.Collapsed;
            }
        }
    }
}