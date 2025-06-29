using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.Models;
using APKInstaller.Helpers;
using APKInstaller.Models;
using APKInstaller.Pages.SettingsPages;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using WinRT;

namespace APKInstaller.ViewModels.SettingsPages
{
    public partial class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly SettingsPage _page;
        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("SettingsPage");

        public static SettingsViewModel Caches { get; set; }

        public static bool IsModified => Package.Current.PublisherDisplayName != "wherewhere"
            || Package.Current.Id.Name != "18184wherewhere.AndroidAppInstaller"
            || (Package.Current.Id.PublisherId != "4v4sx105x6y4r" && Package.Current.Id.PublisherId != "d0s2e6z6qkbn0")
            || (Package.Current.Id.Publisher != "CN=2C3A37C0-35FC-4839-B08C-751C1C1AFBF5" && Package.Current.Id.Publisher != "CN=where");

        public static string WASVersion => Assembly.GetAssembly(typeof(ExtendedActivationKind)).GetName().Version.ToString(3);

        public static string SDKVersion => Assembly.GetAssembly(typeof(PackageSignatureKind)).GetName().Version.ToString();

        public static string WinRTVersion => Assembly.GetAssembly(typeof(TrustLevel)).GetName().Version.ToString(3);

        private DeviceData[] _deviceList;
        public DeviceData[] DeviceList
        {
            get => _deviceList;
            set
            {
                if (_deviceList != value)
                {
                    _deviceList = value;
                    RaisePropertyChangedEvent();
                    if (!IsOnlyWSA) { ChooseDevice(); }
                }
            }
        }

        public bool IsOnlyWSA
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsOnlyWSA);
            set
            {
                if (IsOnlyWSA != value)
                {
                    SettingsHelper.Set(SettingsHelper.IsOnlyWSA, value);
                    _page.SelectDeviceBox.SelectionMode = value ? ListViewSelectionMode.None : ListViewSelectionMode.Single;
                    if (!value) { ChooseDevice(); }
                }
            }
        }

        public static bool ScanPairedDevice
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.ScanPairedDevice);
            set
            {
                if (ScanPairedDevice != value)
                {
                    if (value)
                    {
                        MonitorHelper.InitializeConnectListener();
                        _ = MonitorHelper.ConnectPairedDevice();
                    }
                    else
                    {
                        MonitorHelper.DisposeConnectListener();
                    }
                    SettingsHelper.Set(SettingsHelper.ScanPairedDevice, value);
                }
            }
        }

        public static bool IsCloseADB
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsCloseADB);
            set => SettingsHelper.Set(SettingsHelper.IsCloseADB, value);
        }

        public static bool IsCloseAPP
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsCloseAPP);
            set => SettingsHelper.Set(SettingsHelper.IsCloseAPP, value);
        }

        public static bool ShowDialogs
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.ShowDialogs);
            set => SettingsHelper.Set(SettingsHelper.ShowDialogs, value);
        }

        public static bool ShowProgress
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsUploadAPK);
            set => SettingsHelper.Set(SettingsHelper.IsUploadAPK, value);
        }

        public static bool AutoGetNetAPK
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.AutoGetNetAPK);
            set => SettingsHelper.Set(SettingsHelper.AutoGetNetAPK, value);
        }

        public string ADBPath
        {
            get => SettingsHelper.Get<string>(SettingsHelper.ADBPath);
            set
            {
                if (ADBPath != value)
                {
                    SettingsHelper.Set(SettingsHelper.ADBPath, value);
                    RaisePropertyChangedEvent();
                }
            }
        }

        public bool ShowMessages
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.ShowMessages);
            set
            {
                if (ShowMessages != value)
                {
                    SettingsHelper.Set(SettingsHelper.ShowMessages, value);
                    RaisePropertyChangedEvent();
                }
            }
        }

        public DateTime UpdateDate
        {
            get => SettingsHelper.Get<DateTime>(SettingsHelper.UpdateDate);
            set
            {
                if (UpdateDate != value)
                {
                    SettingsHelper.Set(SettingsHelper.UpdateDate, value);
                    RaisePropertyChangedEvent();
                }
            }
        }

        public static int SelectedTheme
        {
            get => 2 - (int)ThemeHelper.RootTheme;
            set
            {
                if (SelectedTheme != value)
                {
                    ThemeHelper.RootTheme = (ElementTheme)(2 - value);
                }
            }
        }

        public static int SelectedBackdrop
        {
            get => (int)SettingsHelper.Get<BackdropType>(SettingsHelper.SelectedBackdrop);
            set
            {
                if (SelectedBackdrop != value)
                {
                    BackdropType type = (BackdropType)value;
                    SettingsHelper.Set(SettingsHelper.SelectedBackdrop, type);
                    UIHelper.MainWindow.Backdrop.SetBackdrop(type);
                }
            }
        }

        private bool _checkingUpdate;
        public bool CheckingUpdate
        {
            get => _checkingUpdate;
            set
            {
                if (_checkingUpdate != value)
                {
                    _checkingUpdate = value;
                    if (value)
                    {
                        ProgressHelper.SetState(ProgressState.Indeterminate, true);
                    }
                    else
                    {
                        ProgressHelper.SetState(ProgressState.None, true);
                    }
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _gotoUpdateTag;
        public string GotoUpdateTag
        {
            get => _gotoUpdateTag;
            set
            {
                if (_gotoUpdateTag != value)
                {
                    _gotoUpdateTag = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _gotoUpdateVisibility;
        public Visibility GotoUpdateVisibility
        {
            get => _gotoUpdateVisibility;
            set
            {
                if (_gotoUpdateVisibility != value)
                {
                    _gotoUpdateVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _updateStateIsOpen;
        public bool UpdateStateIsOpen
        {
            get => _updateStateIsOpen;
            set
            {
                if (_updateStateIsOpen != value)
                {
                    _updateStateIsOpen = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _updateStateMessage;
        public string UpdateStateMessage
        {
            get => _updateStateMessage;
            set
            {
                if (_updateStateMessage != value)
                {
                    _updateStateMessage = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private InfoBarSeverity _updateStateSeverity;
        public InfoBarSeverity UpdateStateSeverity
        {
            get => _updateStateSeverity;
            set
            {
                if (_updateStateSeverity != value)
                {
                    _updateStateSeverity = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _updateStateTitle;
        public string UpdateStateTitle
        {
            get => _updateStateTitle;
            set
            {
                if (_updateStateTitle != value)
                {
                    _updateStateTitle = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _pairingDevice;
        public bool PairingDevice
        {
            get => _pairingDevice;
            set
            {
                if (_pairingDevice != value)
                {
                    _pairingDevice = value;
                    if (value)
                    {
                        ProgressHelper.SetState(ProgressState.Indeterminate, true);
                    }
                    else
                    {
                        ProgressHelper.SetState(ProgressState.None, true);
                    }
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _connectingDevice;
        public bool ConnectingDevice
        {
            get => _connectingDevice;
            set
            {
                if (_connectingDevice != value)
                {
                    _connectingDevice = value;
                    if (value)
                    {
                        ProgressHelper.SetState(ProgressState.Indeterminate, true);
                    }
                    else
                    {
                        ProgressHelper.SetState(ProgressState.None, true);
                    }
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _connectInfoIsOpen;
        public bool ConnectInfoIsOpen
        {
            get => _connectInfoIsOpen;
            set
            {
                if (_connectInfoIsOpen != value)
                {
                    _connectInfoIsOpen = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private InfoBarSeverity _connectInfoSeverity;
        public InfoBarSeverity ConnectInfoSeverity
        {
            get => _connectInfoSeverity;
            set
            {
                if (_connectInfoSeverity != value)
                {
                    _connectInfoSeverity = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _connectInfoTitle;
        public string ConnectInfoTitle
        {
            get => _connectInfoTitle;
            set
            {
                if (_connectInfoTitle != value)
                {
                    _connectInfoTitle = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _ADBVersion;
        public string ADBVersion
        {
            get => _ADBVersion;
            set
            {
                if (_ADBVersion != value)
                {
                    _ADBVersion = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _aboutTextBlockText;
        public string AboutTextBlockText
        {
            get => _aboutTextBlockText;
            set
            {
                if (_aboutTextBlockText != value)
                {
                    _aboutTextBlockText = value;
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

        public string VersionTextBlockText
        {
            get
            {
                string ver = Package.Current.Id.Version.ToFormattedString(3);
                string name = Package.Current.DisplayName;
                GetAboutTextBlockText();
                return $"{name} v{ver}";
            }
        }

        public static HyperlinkContent[] ConnectHelpers
        {
            get
            {
                string langCode = LanguageHelper.GetPrimaryLanguage();
                ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("InstallPage");
                HyperlinkContent[] values =
                [
                    new(_loader.GetString("NoDevice10"),new Uri($"https://github.com/Paving-Base/APK-Installer/blob/screenshots/Documents/Tutorials/How%20To%20Connect%20Device/How%20To%20Connect%20Device.{langCode}.md")),
                    new(_loader.GetString("HowToConnect"),new Uri($"https://github.com/Paving-Base/APK-Installer/blob/screenshots/Documents/Tutorials/How%20To%20Connect%20WSA/How%20To%20Connect%20WSA.{langCode}.md"))
                ];
                return values;
            }
        }

        public async void GetADBVersion()
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            string version = "Unknown";
            if (File.Exists(ADBPath))
            {
                AdbServerStatus info = AdbServer.Instance.GetStatus();
                if (info.IsRunning)
                {
                    version = info.Version.ToString(3);
                }
            }
            ADBVersion = version;
        }

        private async void GetAboutTextBlockText()
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            string langCode = LanguageHelper.GetPrimaryLanguage();
            Uri dataUri = new($"ms-appx:///Assets/About/About.{langCode}.md");
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            if (file != null)
            {
                string markdown = await FileIO.ReadTextAsync(file);
                AboutTextBlockText = markdown;
            }
        }

        public SettingsViewModel(SettingsPage Page)
        {
            _page = Page;
            Caches = this;
        }

        public void OnDeviceListChanged(object sender, DeviceDataNotifyEventArgs e) => DeviceList = [.. new AdbClient().GetDevices().Where(x => x.State != DeviceState.Offline)];

        public async void CheckUpdate()
        {
            CheckingUpdate = true;
            UpdateInfo info = null;
            try
            {
                info = await UpdateHelper.CheckUpdateAsync("Paving-Base", "APK-Installer").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                UpdateStateIsOpen = true;
                UpdateStateMessage = ex.Message;
                UpdateStateSeverity = InfoBarSeverity.Error;
                GotoUpdateVisibility = Visibility.Collapsed;
                UpdateStateTitle = _loader.GetString("CheckFailed");
                SettingsHelper.LogManager.GetLogger(nameof(SettingsViewModel)).Error(ex.ExceptionToMessage(), ex);
            }
            if (info != null)
            {
                if (info.IsExistNewVersion)
                {
                    UpdateStateIsOpen = true;
                    GotoUpdateTag = info.ReleaseUrl;
                    GotoUpdateVisibility = Visibility.Visible;
                    UpdateStateSeverity = InfoBarSeverity.Warning;
                    UpdateStateTitle = _loader.GetString("FindUpdate");
                    UpdateStateMessage = $"{VersionTextBlockText} -> {info.TagName}";
                }
                else
                {
                    UpdateStateIsOpen = true;
                    GotoUpdateVisibility = Visibility.Collapsed;
                    UpdateStateSeverity = InfoBarSeverity.Success;
                    UpdateStateTitle = _loader.GetString("UpToDate");
                }
            }
            UpdateDate = DateTime.Now;
            CheckingUpdate = false;
        }

        public void ChooseDevice()
        {
            DeviceData device = SettingsHelper.Get<DeviceData>(SettingsHelper.DefaultDevice);
            if (device == null) { return; }
            foreach (DeviceData data in DeviceList)
            {
                if (data.Name == device.Name && data.Model == device.Model && data.Product == device.Product)
                {
                    _page.SelectDeviceBox.SelectedItem = data;
                    break;
                }
            }
        }

        public async void ConnectDevice(string ip)
        {
            ConnectingDevice = true;
            IAdbServer ADBServer = AdbServer.Instance;
            if (!(await ADBServer.GetStatusAsync(CancellationToken.None)).IsRunning)
            {
                try
                {
                    await ADBServer.StartServerAsync(ADBPath, restartServerIfNewer: false, CancellationToken.None);
                    MonitorHelper.Monitor.DeviceListChanged += OnDeviceListChanged;
                }
                catch (Exception ex)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(SettingsViewModel)).Warn(ex.ExceptionToMessage(), ex);
                    ConnectInfoSeverity = InfoBarSeverity.Warning;
                    ConnectInfoTitle = ResourceLoader.GetForViewIndependentUse("InstallPage").GetString("ADBMissing");
                    ConnectInfoIsOpen = true;
                    ConnectingDevice = false;
                    return;
                }
            }
            try
            {
                string results = (await new AdbClient().ConnectAsync(ip)).TrimStart();
                if (results.StartsWith("connected to", StringComparison.InvariantCultureIgnoreCase))
                {
                    ConnectInfoSeverity = InfoBarSeverity.Success;
                    ConnectInfoTitle = results;
                    ConnectInfoIsOpen = true;
                }
                else if (results.StartsWith("cannot connect to", StringComparison.InvariantCultureIgnoreCase))
                {
                    ConnectInfoSeverity = InfoBarSeverity.Error;
                    ConnectInfoTitle = results;
                    ConnectInfoIsOpen = true;
                }
                else if (!string.IsNullOrWhiteSpace(results))
                {
                    ConnectInfoSeverity = InfoBarSeverity.Warning;
                    ConnectInfoTitle = results;
                    ConnectInfoIsOpen = true;
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(SettingsViewModel)).Warn(ex.ExceptionToMessage(), ex);
                ConnectInfoSeverity = InfoBarSeverity.Error;
                ConnectInfoTitle = ex.Message;
                ConnectInfoIsOpen = true;
            }
            ConnectingDevice = false;
        }

        public async void PairDevice(string ip, string code)
        {
            PairingDevice = true;
            IAdbServer ADBServer = AdbServer.Instance;
            if (!(await ADBServer.GetStatusAsync(CancellationToken.None)).IsRunning)
            {
                try
                {
                    await ADBServer.StartServerAsync(ADBPath, restartServerIfNewer: false, CancellationToken.None);
                    MonitorHelper.Monitor.DeviceListChanged += OnDeviceListChanged;
                }
                catch (Exception ex)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(SettingsViewModel)).Warn(ex.ExceptionToMessage(), ex);
                    ConnectInfoSeverity = InfoBarSeverity.Warning;
                    ConnectInfoTitle = ResourceLoader.GetForViewIndependentUse("InstallPage").GetString("ADBMissing");
                    ConnectInfoIsOpen = true;
                    PairingDevice = false;
                    return;
                }
            }
            try
            {
                string results = (await new AdbClient().PairAsync(ip, code)).TrimStart();
                if (results.StartsWith("successfully", StringComparison.InvariantCultureIgnoreCase))
                {
                    ConnectInfoSeverity = InfoBarSeverity.Success;
                    ConnectInfoTitle = results;
                    ConnectInfoIsOpen = true;
                }
                else if (results.StartsWith("failed:", StringComparison.InvariantCultureIgnoreCase))
                {
                    ConnectInfoSeverity = InfoBarSeverity.Error;
                    ConnectInfoTitle = results[8..];
                    ConnectInfoIsOpen = true;
                }
                else if (!string.IsNullOrWhiteSpace(results))
                {
                    ConnectInfoSeverity = InfoBarSeverity.Warning;
                    ConnectInfoTitle = results;
                    ConnectInfoIsOpen = true;
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(SettingsViewModel)).Warn(ex.ExceptionToMessage(), ex);
                ConnectInfoSeverity = InfoBarSeverity.Error;
                ConnectInfoTitle = ex.Message;
                ConnectInfoIsOpen = true;
            }
            PairingDevice = false;
        }

        public async void ChangeADBPath()
        {
            FileOpenPicker FileOpen = new();
            FileOpen.FileTypeFilter.Add(".exe");
            FileOpen.SuggestedStartLocation = PickerLocationId.ComputerFolder;

            // When running on win32, FileSavePicker needs to know the top-level hwnd via IInitializeWithWindow::Initialize.
            if (Window.Current == null)
            {
                IInitializeWithWindow initializeWithWindowWrapper = FileOpen.As<IInitializeWithWindow>();
                HWND hwnd = PInvoke.GetActiveWindow();
                initializeWithWindowWrapper.Initialize(hwnd);
            }

            StorageFile file = await FileOpen.PickSingleFileAsync();
            if (file != null)
            {
                ADBPath = file.Path;
            }
        }
    }

    public record class HyperlinkContent(string Content, Uri NavigateUri);
}
