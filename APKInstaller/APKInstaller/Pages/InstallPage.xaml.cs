using AAPTForNet;
using AAPTForNet.Models;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using APKInstaller.Controls.Dialogs;
using APKInstaller.Helpers;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppLifecycle;
using PortableDownloader;
using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InstallPage : Page, INotifyPropertyChanged
    {
        private DeviceData _device;
        private string _path = @"C:\Users\qq251\Downloads\Programs\Minecraft_1.17.40.06_sign.apk";
        private static bool _wsaonly => SettingsHelper.Get<bool>(SettingsHelper.IsOnlyWSA);
        private new readonly DispatcherQueue DispatcherQueue = DispatcherQueue.GetForCurrentThread();
        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("InstallPage");

        internal string InstallFormat => _loader.GetString("InstallFormat");
        internal string VersionFormat => _loader.GetString("VersionFormat");
        internal string PackageNameFormat => _loader.GetString("PackageNameFormat");

        private ApkInfo _apkInfo = null;
        internal ApkInfo ApkInfo
        {
            get => _apkInfo;
            set
            {
                _apkInfo = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool isOpenApp = SettingsHelper.Get<bool>(SettingsHelper.IsOpenApp);
        internal bool IsOpenApp
        {
            get => isOpenApp;
            set
            {
                SettingsHelper.Set(SettingsHelper.IsOpenApp, value);
                isOpenApp = SettingsHelper.Get<bool>(SettingsHelper.IsOpenApp);
                RaisePropertyChangedEvent();
            }
        }

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

        private bool isInitialized;
        internal bool IsInitialized
        {
            get => isInitialized;
            set
            {
                isInitialized = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public InstallPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is StorageFile ApkFile)
            {
                _path = ApkFile.Path;
            }
            else
            {
                AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();
                switch (args.Kind)
                {
                    case ExtendedActivationKind.File:
                        _path = (args.Data as IFileActivatedEventArgs).Files.First().Path;
                        break;
                    default:
                        break;
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ADBHelper.Monitor.DeviceChanged -= OnDeviceChanged;
        }

        private void OnDeviceChanged(object sender, DeviceDataEventArgs e)
        {
            if (_wsaonly)
            {
                new AdvancedAdbClient().Connect(new DnsEndPoint("127.0.0.1", 58526));
            }
            if (!IsInstalling)
            {
                DispatcherQueue.EnqueueAsync(() =>
                {
                    if (CheckDevice() && _device != null)
                    {
                        CheckAPK();
                    }
                });
            }
        }

        private async void InitialLoadingUI_Loaded(object sender, RoutedEventArgs e)
        {
            await InitilizeADB();
            await InitilizeUI();
        }

        private async Task InitilizeADB()
        {
            WaitProgressText.Text = _loader.GetString("CheckingADB");
            await CheckADB();
            if (!string.IsNullOrEmpty(_path))
            {
                WaitProgressText.Text = _loader.GetString("StartingADB");
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
                            WaitProgressText.Text = _loader.GetString("StartingADB");
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
                        WaitProgressText.Text = _loader.GetString("StartingADB");
                        await Task.Run(() => new AdbServer().StartServer(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"platform-tools\adb.exe"), restartServerIfNewer: false));
                    }
                }
                if (_wsaonly)
                {
                    new AdvancedAdbClient().Connect(new DnsEndPoint("127.0.0.1", 58526));
                }
                ADBHelper.Monitor.DeviceChanged += OnDeviceChanged;
            }
        }

        private async Task InitilizeUI()
        {
            if (!string.IsNullOrEmpty(_path))
            {
                WaitProgressText.Text = _loader.GetString("Loading");
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
                    WaitProgressText.Text = _loader.GetString("Checking");
                    if (CheckDevice() && _device != null)
                    {
                        CheckAPK();
                    }
                    else
                    {
                        ResetUI();
                        ActionButton.IsEnabled = false;
                        ActionButtonText.Text = _loader.GetString("Install");
                        InfoMessageTextBlock.Text = _loader.GetString("WaitingDevice");
                        ActionButton.Visibility = MessagesToUserContainer.Visibility = Visibility.Visible;
                        AppName.Text = string.Format(_loader.GetString("WaitingForInstallFormat"), ApkInfo.AppName);
                        if (_wsaonly)
                        {
                            ContentDialog dialog = new MarkdownDialog()
                            {
                                XamlRoot = XamlRoot,
                                CloseButtonText = _loader.GetString("IKnow"),
                                Title = _loader.GetString("HowToConnect"),
                                DefaultButton = ContentDialogButton.Close,
                                ContentUrl = "https://raw.githubusercontent.com/Paving-Base/APK-Installer/screenshots/Helpers/How%20To%20Connect%20WSA/How%20To%20Connect%20WSA.md",
                            };
                            _ = dialog.ShowAsync();
                        }
                    }
                }
                WaitProgressText.Text = _loader.GetString("Finished");
            }
            else
            {
                ResetUI();
                ApkInfo = new ApkInfo();
                AppName.Text = _loader.GetString("NoPackageWranning");
                AppVersion.Visibility = AppPublisher.Visibility = AppCapabilities.Visibility = Visibility.Collapsed;
            }
            IsInitialized = true;
        }

        private async Task CheckADB(bool force = false)
        {
            if (!force && File.Exists(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"platform-tools\adb.exe")))
            {
                WaitProgressText.Text = _loader.GetString("ADBExist");
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
                    XamlRoot = XamlRoot,
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
                    await Task.Run(async () =>
                    {
                        using Downloader downloader = new Downloader(new DownloaderOptions()
                        {
                            Uri = new Uri("https://dl.google.com/android/repository/platform-tools-latest-windows.zip?hl=zh-cn"),
                            Stream = File.OpenWrite(Path.Combine(ApplicationData.Current.LocalFolder.Path, "platform-tools.zip"))
                        });
                        _ = downloader.Start();
                        while (downloader.TotalSize <= 0 && downloader.IsStarted)
                        {
                            await DispatcherQueue.EnqueueAsync(() => WaitProgressText.Text = _loader.GetString("WaitDownload"));
                            await Task.Delay(500);
                        }
                        await DispatcherQueue.EnqueueAsync(() => WaitProgressRing.IsIndeterminate = false);
                        while (downloader.IsStarted)
                        {
                            await DispatcherQueue.EnqueueAsync(() =>
                            {
                                WaitProgressText.Text = $"{((double)downloader.BytesPerSecond).GetSizeString()}/s";
                                WaitProgressRing.Value = (double)downloader.CurrentSize * 100 / downloader.TotalSize;
                            });
                            await Task.Delay(500);
                        }
                        await DispatcherQueue.EnqueueAsync(() =>
                        {
                            WaitProgressText.Text = _loader.GetString("UnzipADB");
                            WaitProgressRing.IsIndeterminate = true;
                        });
                        IArchive archive = ArchiveFactory.Open(Path.Combine(ApplicationData.Current.LocalFolder.Path, "platform-tools.zip"));
                        await DispatcherQueue.EnqueueAsync(() => WaitProgressRing.IsIndeterminate = false);
                        foreach (IArchiveEntry entry in archive.Entries)
                        {
                            await DispatcherQueue.EnqueueAsync(() =>
                            {
                                WaitProgressRing.Value = (double)(archive.Entries.ToList().IndexOf(entry) + 1) * 100 / archive.Entries.Count();
                                WaitProgressText.Text = string.Format(_loader.GetString("UnzippingFormat"), archive.Entries.ToList().IndexOf(entry) + 1, archive.Entries.Count());
                            });
                            if (!entry.IsDirectory)
                            {
                                entry.WriteToDirectory(ApplicationData.Current.LocalFolder.Path, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                            }
                        }
                        await DispatcherQueue.EnqueueAsync(() =>
                        {
                            WaitProgressRing.IsIndeterminate = true;
                            WaitProgressText.Text = _loader.GetString("UnzipComplete");
                        });
                    });
                }
                else
                {
                    Application.Current.Exit();
                }
            }
        }

        private void ResetUI()
        {
            ActionButton.Visibility =
            SecondaryActionButton.Visibility =
            CancelOperationButton.Visibility =
            TextOutputScrollViewer.Visibility =
            InstallOutputTextBlock.Visibility =
            LaunchWhenReadyCheckbox.Visibility =
            MessagesToUserContainer.Visibility = Visibility.Collapsed;
            ActionButton.IsEnabled =
            SecondaryActionButton.IsEnabled =
            CancelOperationButton.IsEnabled = true;
        }

        private void PackageError(string message)
        {
            ResetUI();
            ApkInfo = new ApkInfo();
            TextOutput.Text = message;
            AppName.Text = _loader.GetString("CannotOpenPackage");
            TextOutputScrollViewer.Visibility = InstallOutputTextBlock.Visibility = Visibility.Visible;
            AppVersion.Visibility = AppPublisher.Visibility = AppCapabilities.Visibility = Visibility.Collapsed;
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
                if (_wsaonly)
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

        private void CheckAPK()
        {
            ResetUI();
            AdvancedAdbClient client = new AdvancedAdbClient();
            if (_device == null)
            {
                ActionButton.IsEnabled = false;
                ActionButtonText.Text = _loader.GetString("Install");
                InfoMessageTextBlock.Text = _loader.GetString("WaitingDevice");
                ActionButton.Visibility = MessagesToUserContainer.Visibility = Visibility.Visible;
                AppName.Text = string.Format(_loader.GetString("WaitingForInstallFormat"), ApkInfo.AppName);
                ContentDialog dialog = new MarkdownDialog()
                {
                    XamlRoot = XamlRoot,
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
                ActionButtonText.Text = _loader.GetString("Install");
                AppName.Text = string.Format(_loader.GetString("InstallFormat"), ApkInfo.AppName);
                ActionButton.Visibility = LaunchWhenReadyCheckbox.Visibility = Visibility.Visible;
            }
            else if (info.VersionCode < int.Parse(ApkInfo.VersionCode))
            {
                ActionButtonText.Text = _loader.GetString("Update");
                AppName.Text = string.Format(_loader.GetString("UpdateFormat"), ApkInfo.AppName);
                ActionButton.Visibility = LaunchWhenReadyCheckbox.Visibility = Visibility.Visible;
            }
            else
            {
                ActionButtonText.Text = _loader.GetString("Reinstall");
                SecondaryActionButtonText.Text = _loader.GetString("Launch");
                AppName.Text = string.Format(_loader.GetString("ReinstallFormat"), ApkInfo.AppName);
                TextOutput.Text = string.Format(_loader.GetString("ReinstallOutput"), ApkInfo.AppName);
                ActionButton.Visibility = SecondaryActionButton.Visibility = TextOutputScrollViewer.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Name)
            {
                case "ActionButton":
                    InstallAPP();
                    break;
                case "SecondaryActionButton":
                    OpenAPP();
                    break;
                case "CancelOperationButton":
                    Application.Current.Exit();
                    break;
            }
        }

        private void OpenAPP() => new AdvancedAdbClient().StartApp(_device, ApkInfo.PackageName);

        private async void InstallAPP()
        {
            try
            {
                IsInstalling = true;
                CancelOperationButtonText.Text = _loader.GetString("Cancel");
                CancelOperationButton.Visibility = LaunchWhenReadyCheckbox.Visibility = Visibility.Visible;
                ActionButton.Visibility = SecondaryActionButton.Visibility = TextOutputScrollViewer.Visibility = InstallOutputTextBlock.Visibility = Visibility.Collapsed;
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
                SecondaryActionButton.Visibility = Visibility.Visible;
                SecondaryActionButtonText.Text = _loader.GetString("Launch");
                CancelOperationButton.Visibility = LaunchWhenReadyCheckbox.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                IsInstalling = false;
                TextOutput.Text = ex.Message;
                TextOutputScrollViewer.Visibility = InstallOutputTextBlock.Visibility = Visibility.Visible;
                ActionButton.Visibility = SecondaryActionButton.Visibility = CancelOperationButton.Visibility = LaunchWhenReadyCheckbox.Visibility = Visibility.Collapsed;
            }
        }
    }
}
