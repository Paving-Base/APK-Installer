using AAPTForNet;
using AAPTForNet.Models;
using APKInstaller.Controls.Dialogs;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.Exceptions;
using APKInstaller.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InstallPage : Page, INotifyPropertyChanged
    {
        private string path = @"C:\Users\qq251\Downloads\Programs\Coolapk-11.4.3-2110131-coolapk-app-sign.apk";
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

        private bool isOpenApp = true;
        internal bool IsOpenApp
        {
            get => isOpenApp;
            set
            {
                isOpenApp = value;
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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ADBHelper.Monitor.DeviceChanged -= OnDeviceChanged;
        }

        private void OnDeviceChanged(object sender, DeviceDataEventArgs e)
        {
            if (!IsInstalling)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    new AdvancedAdbClient().Connect(new DnsEndPoint("127.0.0.1", 58526));
                    if (CheckDevice() && device != null)
                    {
                        CheckAPK();
                    }
                });
            }
        }

        private void InitialLoadingUI_Loaded(object sender, RoutedEventArgs e)
        {
            InitilizeUI();
        }
        private async void InitilizeUI()
        {
            if (!string.IsNullOrEmpty(path))
            {
                WaitProgressText.Text = "Starting ADB Server...";
                await Task.Run(() =>
                {
                    new AdbServer().StartServer($@"{AppDomain.CurrentDomain.BaseDirectory}\platform-tools\adb.exe", restartServerIfNewer: false);
                    new AdvancedAdbClient().Connect(new DnsEndPoint("127.0.0.1", 58526));
                    ADBHelper.Monitor.DeviceChanged += OnDeviceChanged;
                });
                WaitProgressText.Text = "Loading...";
                ApkInfo = await Task.Run(() => { return AAPTool.Decompile(path); });
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
                WaitProgressText.Text = "Finished";
            }
            else
            {
                ResetUI();
                ApkInfo = new ApkInfo();
                AppName.Text = "For proper functioning of the app, try to launch an Android app package or open a package.";
                AppVersion.Visibility = AppPublisher.Visibility = AppCapabilities.Visibility = Visibility.Collapsed;
                FilePickButton.Visibility = Visibility.Visible;
            }
            IsInitialized = true;
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
            FilePickButton.Visibility = Visibility.Collapsed;
            ActionButton.IsEnabled =
            SecondaryActionButton.IsEnabled =
            CancelOperationButton.IsEnabled = true;
        }

        private void ShowError(string message)
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
            List<DeviceData> devices = new AdvancedAdbClient().GetDevices();
            AdvancedAdbClient client = new();
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            if (devices.Count <= 0) { return false; }
            foreach (DeviceData device in devices)
            {
                client.ExecuteRemoteCommand("getprop ro.product.odm.brand", device, receiver);
                if (receiver.ToString().Contains("Windows"))
                {
                    this.device = device ?? this.device;
                    return true;
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
                    ContentUrl = "https://cdn.jsdelivr.net/gh/Paving-Base/APK-Installer@screenshots/Helpers/How%20To%20Connect%20WSA.md",
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
                CancelOperationButton.Visibility = Visibility.Visible;
                ActionButton.Visibility = SecondaryActionButton.Visibility = TextOutputScrollViewer.Visibility = InstallOutputTextBlock.Visibility = Visibility.Collapsed;
                await Task.Run(() =>
                {
                    new PackageManager(new AdvancedAdbClient(), device).InstallPackage(path, true);
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
            catch (AdbException ex)
            {
                TextOutput.Text = ex.Message;
                TextOutputScrollViewer.Visibility = InstallOutputTextBlock.Visibility = Visibility.Visible;
                ActionButton.Visibility = SecondaryActionButton.Visibility = CancelOperationButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void FilePickButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker ApkPicker = new FileOpenPicker();
            ApkPicker.CommitButtonText = "Install";
            ApkPicker.SuggestedStartLocation = PickerLocationId.Downloads;
            ApkPicker.FileTypeFilter.Add(".apk");
            // Get the current window's HWND by passing in the Window object
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(UIHelper.MainWindow);

            // Associate the HWND with the file picker
            WinRT.Interop.InitializeWithWindow.Initialize(ApkPicker, hwnd);
            StorageFile ApkFile = await ApkPicker.PickSingleFileAsync();
            path = ApkFile.Path;
            InitilizeUI();
        }

        private void FilePickButton_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void FilePickButton_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if(items.First() is StorageFile)
                {
                    StorageFile ApkFile = (StorageFile)items.First();
                    if(ApkFile.FileType.ToLower() == ".apk")
                    {
                        path = ApkFile.Path;
                        InitilizeUI();
                    }
                }
            }
        }
    }
}
