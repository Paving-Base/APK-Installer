using AAPTForNet;
using AAPTForNet.Models;
using APKInstaller.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppLifecycle;
using SharpAdbClient;
using SharpAdbClient.DeviceCommands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InstallPage : Page, INotifyPropertyChanged
    {
        private string path;

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
            AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();
            switch(args.Kind)
            {
                case ExtendedActivationKind.File:
                    path = (args.Data as IFileActivatedEventArgs).Files.First().Path;
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
                DispatcherQueue.TryEnqueue(() => CheckAPK());
            }
        }

        private async void InitialLoadingUI_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(path))
            {
                WaitProgressText.Text = "Starting ADB Server...";
                await Task.Run(() => { new AdbServer().StartServer(@$"{AppDomain.CurrentDomain.BaseDirectory}\platform-tools\adb.exe", restartServerIfNewer: false); });
                WaitProgressText.Text = "Loading...";
                ApkInfo = await Task.Run(() =>
                {
                    ADBHelper.Monitor.DeviceChanged += OnDeviceChanged;
                    return AAPTool.Decompile(path);
                });
                WaitProgressText.Text = "Checking...";
                await Task.Run(() => DispatcherQueue.TryEnqueue(() => CheckAPK()));
                WaitProgressText.Text = "Finished";
            }
            else
            {
                ShowError("Failed to locate the app package because the URI is invalid.");
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

        private void CheckAPK()
        {
            ResetUI();
            AdbClient client = new AdbClient();
            if (client.GetDevices() is List<DeviceData> devices && devices.Count > 0)
            {
                DeviceData device = client.GetDevices().First();
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
                    SecondaryActionButtonText.Text = "Close";
                    AppName.Text = $"{ApkInfo.AppName} is already installed";
                    TextOutput.Text = $"A newer version of {ApkInfo.AppName} is already installed.";
                    ActionButton.Visibility = SecondaryActionButton.Visibility = TextOutputScrollViewer.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ActionButton.IsEnabled = false;
                ActionButtonText.Text = "Install";
                InfoMessageTextBlock.Text = "Waiting for Device...";
                AppName.Text = $"Waiting for install {ApkInfo.AppName}";
                ActionButton.Visibility = MessagesToUserContainer.Visibility = Visibility.Visible;
            }
        }
    }
}
