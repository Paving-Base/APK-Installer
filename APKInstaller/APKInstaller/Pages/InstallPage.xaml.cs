using AAPTForNet;
using AAPTForNet.Models;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using APKInstaller.Controls.Dialogs;
using APKInstaller.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PortableDownloader;
using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
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
        private string path /*= @"C:\Users\qq251\Downloads\Programs\MT管理器_2.10.0.apk"*/;
        private bool wsaonly => SettingsHelper.Get<bool>(SettingsHelper.IsOnlyWSA);
        private DeviceData device;

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
                path = ApkFile.Path;
            }
            else
            {
                IActivatedEventArgs args = AppInstance.GetActivatedEventArgs();
                switch (args.Kind)
                {
                    case ActivationKind.File:
                        path = (args as IFileActivatedEventArgs).Files.First().Path;
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
            if (wsaonly)
            {
                new AdvancedAdbClient().Connect(new DnsEndPoint("127.0.0.1", 58526));
            }
            if (!IsInstalling)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    if (CheckDevice() && device != null)
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
            WaitProgressText.Text = "Checking ADB...";
            await CheckADB();
            if (!string.IsNullOrEmpty(path))
            {
                WaitProgressText.Text = "Starting ADB Server...";
                try
                {
                    await Task.Run(() => new AdbServer().StartServer(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"platform-tools\adb.exe"), restartServerIfNewer: false));
                }
                catch
                {
                    await CheckADB(true);
                    WaitProgressText.Text = "Starting ADB Server...";
                    await Task.Run(() => new AdbServer().StartServer(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"platform-tools\adb.exe"), restartServerIfNewer: false));
                }
                if (wsaonly)
                {
                    new AdvancedAdbClient().Connect(new DnsEndPoint("127.0.0.1", 58526));
                }
                ADBHelper.Monitor.DeviceChanged += OnDeviceChanged;
            }
        }

        private async Task InitilizeUI()
        {
            if (!string.IsNullOrEmpty(path))
            {
                WaitProgressText.Text = "Loading...";
                try
                {
                    ApkInfo = await Task.Run(() => { return AAPTool.Decompile(path); });
                }
                catch (Exception ex)
                {
                    PackageError(ex.Message);
                    IsInitialized = true;
                    return;
                }
                if (string.IsNullOrEmpty(ApkInfo.PackageName))
                {
                    PackageError("The package is either corrupted or invalid.");
                }
                else
                {
                    WaitProgressText.Text = "Checking...";
                    if (CheckDevice() && device != null)
                    {
                        CheckAPK();
                    }
                    else
                    {
                        ResetUI();
                        ActionButton.IsEnabled = false;
                        ActionButtonText.Text = "Install";
                        InfoMessageTextBlock.Text = "Waiting for Device...";
                        AppName.Text = $"Waiting for install {ApkInfo.AppName}";
                        ActionButton.Visibility = MessagesToUserContainer.Visibility = Visibility.Visible;
                        if (wsaonly)
                        {
                            ContentDialog dialog = new MarkdownDialog()
                            {
                                XamlRoot = XamlRoot,
                                CloseButtonText = "I Know",
                                Title = "How to connect WSA?",
                                DefaultButton = ContentDialogButton.Close,
                                ContentUrl = "https://raw.githubusercontent.com/Paving-Base/APK-Installer/screenshots/Helpers/How%20To%20Connect%20WSA/How%20To%20Connect%20WSA.md",
                            };
                            _ = dialog.ShowAsync();
                        }
                    }
                }
                WaitProgressText.Text = "Finished";
            }
            else
            {
                ResetUI();
                ApkInfo = new ApkInfo();
                AppName.Text = "For proper functioning of the app, try to launch an Android app package or open a package.";
                AppVersion.Visibility = AppPublisher.Visibility = AppCapabilities.Visibility = Visibility.Collapsed;
            }
            IsInitialized = true;
        }

        private async Task CheckADB(bool force = false)
        {
            if (!force && File.Exists(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"platform-tools\adb.exe")))
            {
                WaitProgressText.Text = "ADB Exist";
            }
            else
            {
                StackPanel StackPanel = new StackPanel();
                StackPanel.Children.Add(
                    new TextBlock()
                    {
                        TextWrapping = TextWrapping.Wrap,
                        Text = "ADB, the And roid Debug Bridge, is needed for APK Installer to work. Click OK to download it. By downloading, you accept the Google Terms and Conditions for ADB."
                    });
                StackPanel.Children.Add(
                    new HyperlinkButton()
                    {
                        Content = "Click here to read them",
                        NavigateUri = new Uri("https://developer.android.google.cn/studio/releases/platform-tools?hl=zh-cn")
                    });
                ContentDialog dialog = new ContentDialog()
                {
                    XamlRoot = XamlRoot,
                    Title = "ADB is missing",
                    PrimaryButtonText = "OK",
                    CloseButtonText = "Cancel",
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
                            _ = DispatcherQueue.TryEnqueue(() => WaitProgressText.Text = "Wait for download...");
                            await Task.Delay(500);
                        }
                        _ = DispatcherQueue.TryEnqueue(() => WaitProgressRing.IsIndeterminate = false);
                        while (downloader.IsStarted)
                        {
                            _ = DispatcherQueue.TryEnqueue(() =>
                              {
                                  WaitProgressText.Text = $"{((double)downloader.BytesPerSecond).GetSizeString()}/s";
                                  WaitProgressRing.Value = (double)downloader.CurrentSize * 100 / downloader.TotalSize;
                              });
                            await Task.Delay(500);
                        }
                        _ = DispatcherQueue.TryEnqueue(() =>
                          {
                              WaitProgressText.Text = "Unzip ADB...";
                              WaitProgressRing.IsIndeterminate = true;
                          });
                        IArchive archive = ArchiveFactory.Open(Path.Combine(ApplicationData.Current.LocalFolder.Path, "platform-tools.zip"));
                        _ = DispatcherQueue.TryEnqueue(() => WaitProgressRing.IsIndeterminate = false);
                        foreach (IArchiveEntry entry in archive.Entries)
                        {
                            _ = DispatcherQueue.TryEnqueue(() =>
                            {
                                WaitProgressRing.Value = (double)(archive.Entries.ToList().IndexOf(entry) + 1) * 100 / archive.Entries.Count();
                                WaitProgressText.Text = $"Unzipping {archive.Entries.ToList().IndexOf(entry) + 1}/{archive.Entries.Count()} ...";
                            });
                            if (!entry.IsDirectory)
                            {
                                entry.WriteToDirectory(ApplicationData.Current.LocalFolder.Path, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                            }
                        }
                        _ = DispatcherQueue.TryEnqueue(() =>
                          {
                              WaitProgressRing.IsIndeterminate = true;
                              WaitProgressText.Text = "Unzip complete";
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
            AppName.Text = "Cannot open app package";
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
                if (wsaonly)
                {
                    client.ExecuteRemoteCommand("getprop ro.boot.hardware", device, receiver);
                    if (receiver.ToString().Contains("windows"))
                    {
                        this.device = device ?? this.device;
                        return true;
                    }
                }
                else
                {
                    DeviceData data = SettingsHelper.Get<DeviceData>(SettingsHelper.DefaultDevice);
                    if (data.Name == device.Name && data.Model == device.Model && data.Product == device.Product)
                    {
                        this.device = data;
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
            if (device == null)
            {
                ActionButton.IsEnabled = false;
                ActionButtonText.Text = "Install";
                InfoMessageTextBlock.Text = "Waiting for Device...";
                AppName.Text = $"Waiting for install {ApkInfo.AppName}";
                ActionButton.Visibility = MessagesToUserContainer.Visibility = Visibility.Visible;
                ContentDialog dialog = new MarkdownDialog()
                {
                    XamlRoot = XamlRoot,
                    ContentUrl = "https://raw.githubusercontent.com/Paving-Base/APK-Installer/screenshots/Helpers/How%20To%20Connect%20WSA/How%20To%20Connect%20WSA.md",
                };
                _ = dialog.ShowAsync();
                return;
            }
            PackageManager manager = new PackageManager(client, device);
            var info = manager.GetVersionInfo(ApkInfo.PackageName);
            if (info == null)
            {
                ActionButtonText.Text = "Install";
                AppName.Text = $"Install {ApkInfo.AppName}?";
                ActionButton.Visibility = LaunchWhenReadyCheckbox.Visibility = Visibility.Visible;
            }
            else if (info.VersionCode < int.Parse(ApkInfo.VersionCode))
            {
                ActionButtonText.Text = "Update";
                AppName.Text = $"Update {ApkInfo.AppName}?";
                ActionButton.Visibility = LaunchWhenReadyCheckbox.Visibility = Visibility.Visible;
            }
            else
            {
                ActionButtonText.Text = "Reinstall";
                SecondaryActionButtonText.Text = "Launch";
                AppName.Text = $"{ApkInfo.AppName} is already installed";
                TextOutput.Text = $"A newer version of {ApkInfo.AppName} is already installed.";
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

        private void OpenAPP() => new AdvancedAdbClient().StartApp(device, ApkInfo.PackageName);

        private async void InstallAPP()
        {
            try
            {
                IsInstalling = true;
                CancelOperationButtonText.Text = "Cancel";
                CancelOperationButton.Visibility = LaunchWhenReadyCheckbox.Visibility = Visibility.Visible;
                ActionButton.Visibility = SecondaryActionButton.Visibility = TextOutputScrollViewer.Visibility = InstallOutputTextBlock.Visibility = Visibility.Collapsed;
                await Task.Run(() =>
                {
                    new AdvancedAdbClient().Install(device, File.Open(path, FileMode.Open, FileAccess.Read));
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
                SecondaryActionButtonText.Text = "Launch";
                SecondaryActionButton.Visibility = Visibility.Visible;
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
