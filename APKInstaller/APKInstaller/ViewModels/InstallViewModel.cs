using AAPTForNet;
using AAPTForNet.Models;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.DeviceCommands.Models;
using AdvancedSharpAdbClient.Models;
using AdvancedSharpAdbClient.Receivers;
using APKInstaller.Controls.Dialogs;
using APKInstaller.Helpers;
using APKInstaller.Models;
using APKInstaller.Pages;
using APKInstaller.Pages.SettingsPages;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Connectivity;
using Downloader;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using WinRT;
using DownloadProgressChangedEventArgs = Downloader.DownloadProgressChangedEventArgs;

namespace APKInstaller.ViewModels
{
    public partial class InstallViewModel : INotifyPropertyChanged
    {
        private InstallPage _page;
        private DeviceData _device;
        private readonly ProtocolForResultsOperation _operation;
        private static readonly string APKTemp = Path.Combine(CachesHelper.TempPath, "NetAPKTemp.apk");
        private static readonly string ADBTemp = Path.Combine(CachesHelper.TempPath, "platform-tools.zip");

#if !DEBUG
        private Uri _url;
        private string _path = string.Empty;
#else
        private Uri _url = new("apkinstaller:?source=https://dl.coolapk.com/down?pn=com.coolapk.market&id=NDU5OQ&h=46bb9d98&from=from-web");
        private string _path = @"C:\Users\qq251\Downloads\Programs\weixin8060android2860_0x28003c39_arm64.apk";
#endif
        private bool NetAPKExist => _path != APKTemp || File.Exists(_path);

        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("InstallPage");

        public static InstallViewModel Caches { get; set; }

        public string AppLocaleName = string.Empty;
        public string InstallFormat => _loader.GetString("InstallFormat");
        public string VersionFormat => _loader.GetString("VersionFormat");
        public string PackageNameFormat => _loader.GetString("PackageNameFormat");

        private static bool IsOnlyWSA => SettingsHelper.Get<bool>(SettingsHelper.IsOnlyWSA);
        private static bool IsCloseAPP => SettingsHelper.Get<bool>(SettingsHelper.IsCloseAPP);
        private static bool IsUploadAPK => SettingsHelper.Get<bool>(SettingsHelper.IsUploadAPK);
        private static bool ShowDialogs => SettingsHelper.Get<bool>(SettingsHelper.ShowDialogs);
        private static bool AutoGetNetAPK => SettingsHelper.Get<bool>(SettingsHelper.AutoGetNetAPK);
        private static bool ScanPairedDevice => SettingsHelper.Get<bool>(SettingsHelper.ScanPairedDevice);

        public bool IsADBReady { get; private set; }

        private ApkInfo _apkInfo = null;
        public ApkInfo ApkInfo
        {
            get => _apkInfo;
            set
            {
                if (_apkInfo != value)
                {
                    _apkInfo = value;
                    RaisePropertyChangedEvent();
                }
            }
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

        public static bool IsOpenApp
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsOpenApp);
            set => SettingsHelper.Set(SettingsHelper.IsOpenApp, value);
        }

        private bool _isInstalling;
        public bool IsInstalling
        {
            get => _isInstalling;
            set
            {
                if (_isInstalling != value)
                {
                    _isInstalling = value;
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

        private bool _isInitialized;
        public bool IsInitialized
        {
            get => _isInitialized;
            set
            {
                if (_isInitialized != value)
                {
                    _isInitialized = value;
                    if (value)
                    {
                        ProgressHelper.SetState(ProgressState.None, true);
                    }
                    else
                    {
                        ProgressHelper.SetState(ProgressState.Indeterminate, true);
                    }
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _appName;
        public string AppName
        {
            get => _appName;
            set
            {
                if (_appName != value)
                {
                    _appName = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _textOutput;
        public string TextOutput
        {
            get => _textOutput;
            set
            {
                if (_textOutput != value)
                {
                    _textOutput = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _infoMessage;
        public string InfoMessage
        {
            get => _infoMessage;
            set
            {
                if (_infoMessage != value)
                {
                    _infoMessage = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _progressText;
        public string ProgressText
        {
            get => _progressText;
            set
            {
                if (_progressText != value)
                {
                    _progressText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _actionButtonEnable;
        public bool ActionButtonEnable
        {
            get => _actionButtonEnable;
            set
            {
                if (_actionButtonEnable != value)
                {
                    _actionButtonEnable = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _secondaryActionButtonEnable;
        public bool SecondaryActionButtonEnable
        {
            get => _secondaryActionButtonEnable;
            set
            {
                if (_secondaryActionButtonEnable != value)
                {
                    _secondaryActionButtonEnable = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _fileSelectButtonEnable;
        public bool FileSelectButtonEnable
        {
            get => _fileSelectButtonEnable;
            set
            {
                if (_fileSelectButtonEnable != value)
                {
                    _fileSelectButtonEnable = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _downloadButtonEnable;
        public bool DownloadButtonEnable
        {
            get => _downloadButtonEnable;
            set
            {
                if (_downloadButtonEnable != value)
                {
                    _downloadButtonEnable = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _deviceSelectButtonEnable;
        public bool DeviceSelectButtonEnable
        {
            get => _deviceSelectButtonEnable;
            set
            {
                if (_deviceSelectButtonEnable != value)
                {
                    _deviceSelectButtonEnable = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _cancelOperationButtonEnable;
        public bool CancelOperationButtonEnable
        {
            get => _cancelOperationButtonEnable;
            set
            {
                if (_cancelOperationButtonEnable != value)
                {
                    _cancelOperationButtonEnable = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _waitProgressText;
        public string WaitProgressText
        {
            get => _waitProgressText;
            set
            {
                if (_waitProgressText != value)
                {
                    _waitProgressText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private double _waitProgressValue = 0;
        public double WaitProgressValue
        {
            get => _waitProgressValue;
            set
            {
                if (_waitProgressValue != value)
                {
                    _waitProgressValue = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private double _appxInstallBarValue = 0;
        public double AppxInstallBarValue
        {
            get => _appxInstallBarValue;
            set
            {
                if (_appxInstallBarValue != value)
                {
                    _appxInstallBarValue = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _waitProgressIndeterminate = true;
        public bool WaitProgressIndeterminate
        {
            get => _waitProgressIndeterminate;
            set
            {
                if (_waitProgressIndeterminate != value)
                {
                    _waitProgressIndeterminate = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _appxInstallBarIndeterminate = true;
        public bool AppxInstallBarIndeterminate
        {
            get => _appxInstallBarIndeterminate;
            set
            {
                if (_appxInstallBarIndeterminate != value)
                {
                    _appxInstallBarIndeterminate = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _actionButtonText;
        public string ActionButtonText
        {
            get => _actionButtonText;
            set
            {
                if (_actionButtonText != value)
                {
                    _actionButtonText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _secondaryActionButtonText;
        public string SecondaryActionButtonText
        {
            get => _secondaryActionButtonText;
            set
            {
                if (_secondaryActionButtonText != value)
                {
                    _secondaryActionButtonText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _fileSelectButtonText;
        public string FileSelectButtonText
        {
            get => _fileSelectButtonText;
            set
            {
                if (_fileSelectButtonText != value)
                {
                    _fileSelectButtonText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _downloadButtonText;
        public string DownloadButtonText
        {
            get => _downloadButtonText;
            set
            {
                if (_downloadButtonText != value)
                {
                    _downloadButtonText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _deviceSelectButtonText;
        public string DeviceSelectButtonText
        {
            get => _deviceSelectButtonText;
            set
            {
                if (_deviceSelectButtonText != value)
                {
                    _deviceSelectButtonText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _cancelOperationButtonText;
        public string CancelOperationButtonText
        {
            get => _cancelOperationButtonText;
            set
            {
                if (_cancelOperationButtonText != value)
                {
                    _cancelOperationButtonText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _textOutputVisibility = Visibility.Collapsed;
        public Visibility TextOutputVisibility
        {
            get => _textOutputVisibility;
            set
            {
                if (_textOutputVisibility != value)
                {
                    _textOutputVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _installOutputVisibility = Visibility.Collapsed;
        public Visibility InstallOutputVisibility
        {
            get => _installOutputVisibility;
            set
            {
                if (_installOutputVisibility != value)
                {
                    _installOutputVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _actionVisibility = Visibility.Collapsed;
        public Visibility ActionVisibility
        {
            get => _actionVisibility;
            set
            {
                if (_actionVisibility != value)
                {
                    _actionVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _secondaryActionVisibility = Visibility.Collapsed;
        public Visibility SecondaryActionVisibility
        {
            get => _secondaryActionVisibility;
            set
            {
                if (_secondaryActionVisibility != value)
                {
                    _secondaryActionVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _fileSelectVisibility = Visibility.Collapsed;
        public Visibility FileSelectVisibility
        {
            get => _fileSelectVisibility;
            set
            {
                if (_fileSelectVisibility != value)
                {
                    _fileSelectVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _downloadVisibility = Visibility.Collapsed;
        public Visibility DownloadVisibility
        {
            get => _downloadVisibility;
            set
            {
                if (_downloadVisibility != value)
                {
                    _downloadVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _deviceSelectVisibility = Visibility.Collapsed;
        public Visibility DeviceSelectVisibility
        {
            get => _deviceSelectVisibility;
            set
            {
                if (_deviceSelectVisibility != value)
                {
                    _deviceSelectVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _cancelOperationVisibility = Visibility.Collapsed;
        public Visibility CancelOperationVisibility
        {
            get => _cancelOperationVisibility;
            set
            {
                if (_cancelOperationVisibility != value)
                {
                    _cancelOperationVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _messagesToUserVisibility = Visibility.Collapsed;
        public Visibility MessagesToUserVisibility
        {
            get => _messagesToUserVisibility;
            set
            {
                if (_messagesToUserVisibility != value)
                {
                    _messagesToUserVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _launchWhenReadyVisibility = Visibility.Collapsed;
        public Visibility LaunchWhenReadyVisibility
        {
            get => _launchWhenReadyVisibility;
            set
            {
                if (_launchWhenReadyVisibility != value)
                {
                    _launchWhenReadyVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _appVersionVisibility;
        public Visibility AppVersionVisibility
        {
            get => _appVersionVisibility;
            set
            {
                if (_appVersionVisibility != value)
                {
                    _appVersionVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _appPublisherVisibility;
        public Visibility AppPublisherVisibility
        {
            get => _appPublisherVisibility;
            set
            {
                if (_appPublisherVisibility != value)
                {
                    _appPublisherVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _appCapabilitiesVisibility;
        public Visibility AppCapabilitiesVisibility
        {
            get => _appCapabilitiesVisibility;
            set
            {
                if (_appCapabilitiesVisibility != value)
                {
                    _appCapabilitiesVisibility = value;
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

        public InstallViewModel(Uri Url, InstallPage Page, ProtocolForResultsOperation Operation = null)
        {
            _url = Url;
            _page = Page;
            Caches = this;
            _path = APKTemp;
            _operation = Operation;
        }

        public InstallViewModel(string Path, InstallPage Page, ProtocolForResultsOperation Operation = null)
        {
            _page = Page;
            Caches = this;
            _operation = Operation;
            _path = string.IsNullOrWhiteSpace(Path) ? _path : Path;
        }

        public static void SetPage(InstallPage Page) => Caches._page = Page;

        public async Task Refresh(bool force = true)
        {
            IsInitialized = false;
            WaitProgressText = _loader.GetString("Loading");
            //await OnFirstRun();
            try
            {
                if (force)
                {
                    ProgressHelper.SetState(ProgressState.Indeterminate, true);
                    await InitializeADB();
                    await InitializeUI();
                }
                else
                {
                    await ReinitializeUI();
                    IsInitialized = true;
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(InstallViewModel)).Error(ex.ExceptionToMessage(), ex);
                PackageError(ex.Message);
                IsInstalling = false;
            }
            IsInitialized = true;
        }

        private async Task OnFirstRun()
        {
            if (SettingsHelper.Get<bool>(SettingsHelper.IsFirstRun))
            {
                ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("InstallPage");
                MarkdownDialog dialog = new()
                {
                    Title = _loader.GetString("Welcome"),
                    XamlRoot = _page?.XamlRoot,
                    DefaultButton = ContentDialogButton.Close,
                    CloseButtonText = _loader.GetString("IKnow"),
                    ContentTask = async () =>
                    {
                        string langcode = LanguageHelper.GetCurrentLanguage();
                        Uri dataUri = new($"ms-appx:///String/{langcode}/About.md");
                        StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
                        return await FileIO.ReadTextAsync(file);
                    }
                };
                _ = await _page?.DispatcherQueue.EnqueueAsync(async () => await dialog.ShowAsync());
                SettingsHelper.Set(SettingsHelper.IsFirstRun, false);
            }
        }

        private bool CheckADB()
        {
            if (!File.Exists(ADBPath))
            {
                ADBPath = @"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe";
                return File.Exists(ADBPath);
            }
            return true;
        }

        public async Task InitADBFile()
        {
            IsInitialized = false;
            WaitProgressText = _loader.GetString("Loading");
            try
            {
            checkadb:
                if (!File.Exists(ADBPath))
                {
                    StackPanel StackPanel = new();
                    StackPanel.Children.Add(
                        new TextBlock
                        {
                            TextWrapping = TextWrapping.Wrap,
                            Text = _loader.GetString("AboutADB")
                        });
                    StackPanel.Children.Add(
                        new HyperlinkButton
                        {
                            Content = _loader.GetString("ClickToRead"),
                            NavigateUri = new Uri("https://developer.android.google.cn/studio/releases/platform-tools")
                        });
                    ContentDialog dialog = new()
                    {
                        XamlRoot = _page?.XamlRoot,
                        Title = _loader.GetString("ADBMissing"),
                        PrimaryButtonText = _loader.GetString("Download"),
                        SecondaryButtonText = _loader.GetString("Select"),
                        CloseButtonText = _loader.GetString("Cancel"),
                        Content = new ScrollViewer
                        {
                            Content = StackPanel
                        },
                        DefaultButton = ContentDialogButton.Primary
                    };
                    ProgressHelper.SetState(ProgressState.None, true);
                    ContentDialogResult result = await dialog.ShowAsync();
                    ProgressHelper.SetState(ProgressState.Indeterminate, true);
                    if (result == ContentDialogResult.Primary)
                    {
                    downloadadb:
                        if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                        {
                            try
                            {
                                await DownloadADB();
                            }
                            catch (Exception ex)
                            {
                                SettingsHelper.LogManager.GetLogger(nameof(InstallViewModel)).Error(ex.ExceptionToMessage(), ex);
                                ContentDialog dialogs = new()
                                {
                                    XamlRoot = _page?.XamlRoot,
                                    Title = _loader.GetString("DownloadFailed"),
                                    PrimaryButtonText = _loader.GetString("Retry"),
                                    CloseButtonText = _loader.GetString("Cancel"),
                                    Content = new TextBlock { Text = ex.Message },
                                    DefaultButton = ContentDialogButton.Primary
                                };
                                ProgressHelper.SetState(ProgressState.None, true);
                                ContentDialogResult results = await dialogs.ShowAsync();
                                ProgressHelper.SetState(ProgressState.Indeterminate, true);
                                if (results == ContentDialogResult.Primary)
                                {
                                    goto downloadadb;
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                        else
                        {
                            ContentDialog dialogs = new()
                            {
                                XamlRoot = _page?.XamlRoot,
                                Title = _loader.GetString("NoInternet"),
                                PrimaryButtonText = _loader.GetString("Retry"),
                                CloseButtonText = _loader.GetString("Cancel"),
                                Content = new TextBlock { Text = _loader.GetString("NoInternetInfo") },
                                DefaultButton = ContentDialogButton.Primary
                            };
                            ProgressHelper.SetState(ProgressState.None, true);
                            ContentDialogResult results = await dialogs.ShowAsync();
                            ProgressHelper.SetState(ProgressState.Indeterminate, true);
                            if (results == ContentDialogResult.Primary)
                            {
                                goto checkadb;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    else if (result == ContentDialogResult.Secondary)
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
                    else
                    {
                        SendResults(new Exception(_loader.GetString("ADBMissing")));
                        Application.Current.Exit();
                        return;
                    }
                }
                await ThreadSwitcher.ResumeBackgroundAsync();
                await InitializeADB();
                await ReinitializeUI();
            }
            finally
            {
                IsInitialized = true;
            }
        }

        private async Task DownloadADB()
        {
            if (!Directory.Exists(ADBTemp[..ADBTemp.LastIndexOf('\\')]))
            {
                _ = Directory.CreateDirectory(ADBTemp[..ADBTemp.LastIndexOf('\\')]);
            }
            else if (Directory.Exists(ADBTemp))
            {
                Directory.Delete(ADBTemp, true);
            }
            using (DownloadService downloader = new(DownloadHelper.Configuration))
            {
                bool IsCompleted = false;
                Exception exception = null;
                long ReceivedBytesSize = 0;
                long TotalBytesToReceive = 0;
                double ProgressPercentage = 0;
                double BytesPerSecondSpeed = 0;
                void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
                {
                    exception = e.Error;
                    IsCompleted = true;
                }
                void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
                {
                    ReceivedBytesSize = e.ReceivedBytesSize;
                    ProgressPercentage = e.ProgressPercentage;
                    TotalBytesToReceive = e.TotalBytesToReceive;
                    BytesPerSecondSpeed = e.BytesPerSecondSpeed;
                }
                downloader.DownloadFileCompleted += OnDownloadFileCompleted;
                downloader.DownloadProgressChanged += OnDownloadProgressChanged;
                downloadadb:
                WaitProgressText = _loader.GetString("WaitDownload");
                _ = downloader.DownloadFileTaskAsync("https://dl.google.com/android/repository/platform-tools-latest-windows.zip", ADBTemp);
                while (TotalBytesToReceive <= 0)
                {
                    await Task.Delay(1);
                    if (IsCompleted)
                    {
                        goto downloadfinish;
                    }
                }
                WaitProgressIndeterminate = false;
                ProgressHelper.SetState(ProgressState.Normal, true);
                while (!IsCompleted)
                {
                    ProgressHelper.SetValue(Convert.ToInt32(ReceivedBytesSize), Convert.ToInt32(TotalBytesToReceive), true);
                    WaitProgressText = $"{((double)BytesPerSecondSpeed).GetSizeString()}/s";
                    WaitProgressValue = ProgressPercentage;
                    await Task.Delay(1);
                }
                ProgressHelper.SetState(ProgressState.Indeterminate, true);
                WaitProgressIndeterminate = true;
                WaitProgressValue = 0;
                downloadfinish:
                if (exception != null)
                {
                    ProgressHelper.SetState(ProgressState.Error, true);
                    ContentDialog dialog = new()
                    {
                        XamlRoot = _page?.XamlRoot,
                        Content = exception.Message,
                        Title = _loader.GetString("DownloadFailed"),
                        PrimaryButtonText = _loader.GetString("Retry"),
                        CloseButtonText = _loader.GetString("Cancel"),
                        DefaultButton = ContentDialogButton.Primary
                    };
                    ContentDialogResult result = await dialog.ShowAsync();
                    ProgressHelper.SetState(ProgressState.Indeterminate, true);
                    if (result == ContentDialogResult.Primary)
                    {
                        goto downloadadb;
                    }
                    else
                    {
                        SendResults(new Exception($"ADB {_loader.GetString("DownloadFailed")}"));
                        Application.Current.Exit();
                        return;
                    }
                }
                downloader.DownloadProgressChanged -= OnDownloadProgressChanged;
                downloader.DownloadFileCompleted -= OnDownloadFileCompleted;
            }
            WaitProgressText = _loader.GetString("UnzipADB");
            await Task.Delay(1);
            using (IArchive archive = ArchiveFactory.Open(ADBTemp))
            {
                ProgressHelper.SetState(ProgressState.Normal, true);
                WaitProgressIndeterminate = false;
                int Progressed = 0;
                bool IsCompleted = false;
                double ProgressPercentage = 0;
                int TotalCount = archive.Entries.Count();
                _ = Task.Run(() =>
                {
                    foreach (IArchiveEntry entry in archive.Entries)
                    {
                        Progressed = archive.Entries.ToList().IndexOf(entry) + 1;
                        ProgressPercentage = archive.Entries.GetProgressValue(entry);
                        if (!entry.IsDirectory)
                        {
                            entry.WriteToDirectory(ApplicationData.Current.LocalFolder.Path, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                        }
                    }
                    IsCompleted = true;
                });
                while (!IsCompleted)
                {
                    WaitProgressValue = ProgressPercentage;
                    ProgressHelper.SetValue(Progressed, TotalCount, true);
                    WaitProgressText = string.Format(_loader.GetString("UnzippingFormat"), Progressed, TotalCount);
                    await Task.Delay(1);
                }
                WaitProgressValue = 0;
                WaitProgressIndeterminate = true;
                WaitProgressText = _loader.GetString("UnzipComplete");
                ProgressHelper.SetState(ProgressState.Indeterminate, true);
            }
            ADBPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, @"platform-tools\adb.exe");
        }

        private async Task InitializeADB()
        {
            WaitProgressText = _loader.GetString("Loading");
            if (!string.IsNullOrWhiteSpace(_path) || _url != null)
            {
                IAdbServer ADBServer = AdbServer.Instance;
                if (!ADBServer.GetStatus().IsRunning)
                {
                    WaitProgressText = _loader.GetString("CheckingADB");
                    if (CheckADB())
                    {
                        Process[] processes = Process.GetProcessesByName("adb");
                        WaitProgressText = _loader.GetString("StartingADB");
                        try
                        {
                            await ADBServer.StartServerAsync(processes is [{ MainModule.FileName: string file }, ..] ? file : ADBPath, restartServerIfNewer: false, CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            SettingsHelper.LogManager.GetLogger(nameof(InstallViewModel)).Warn(ex.ExceptionToMessage(), ex);
                            if (processes != null && processes.Length != 0)
                            {
                                foreach (Process process in processes)
                                {
                                    process.Kill();
                                }
                                processes = null;
                            }
                            IsADBReady = false;
                            return;
                        }
                        IsADBReady = true;
                    }
                    else
                    {
                        IsADBReady = false;
                        return;
                    }
                }
                WaitProgressText = _loader.GetString("Loading");
                if (!await CheckDevice())
                {
                    WaitProgressText = _loader.GetString("ConnectPairedDevices");
                    if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                    {
                        if (ScanPairedDevice)
                        {
                            MonitorHelper.InitializeConnectListener();
                        }

                        if (!await CheckDevice())
                        {
                            _ = new AdbClient().ConnectAsync(new DnsEndPoint("127.0.0.1", 58526));
                        }
                    }
                    else
                    {
                        _ = new AdbClient().ConnectAsync(new DnsEndPoint("127.0.0.1", 58526));
                    }
                    WaitProgressText = _loader.GetString("Loading");
                }
                MonitorHelper.Monitor.DeviceListChanged += OnDeviceListChanged;
            }
        }

        private async Task InitializeUI()
        {
            if (!string.IsNullOrWhiteSpace(_path) || _url != null)
            {
                WaitProgressText = _loader.GetString("Loading");
                if (NetAPKExist)
                {
                    WaitProgressText = _loader.GetString("Analysis");
                    try
                    {
                        ApkInfo = await Task.Run(() => { return AAPTool.Decompile(_path); });
                        AppLocaleName = ApkInfo.GetLocaleLabel();
                    }
                    catch (Exception ex)
                    {
                        SettingsHelper.LogManager.GetLogger(nameof(InstallViewModel)).Error(ex.ExceptionToMessage(), ex);
                        PackageError(ex.Message);
                        IsInitialized = true;
                        return;
                    }
                    WaitProgressText = _loader.GetString("Loading");
                }
                else
                {
                    ApkInfo ??= new ApkInfo();
                }
                if ((ApkInfo?.IsEmpty).GetValueOrDefault(true) && NetAPKExist)
                {
                    PackageError(_loader.GetString("InvalidPackage"));
                }
                else
                {
                    checkdevice:
                    WaitProgressText = _loader.GetString("Checking");
                    if (await CheckDevice() && _device != null)
                    {
                        if (NetAPKExist)
                        {
                            await CheckAPK();
                        }
                        else
                        {
                            ResetUI();
                            CheckOnlinePackage();
                        }
                    }
                    else
                    {
                        ResetUI();
                        if (NetAPKExist)
                        {
                            ActionButtonEnable = false;
                            ActionButtonText = _loader.GetString("Install");
                            InfoMessage = _loader.GetString("WaitingDevice");
                            DeviceSelectButtonText = _loader.GetString("Devices");
                            AppName = string.Format(_loader.GetString("WaitingForInstallFormat"), AppLocaleName);
                            ActionVisibility = DeviceSelectVisibility = MessagesToUserVisibility = Visibility.Visible;
                        }
                        else
                        {
                            CheckOnlinePackage();
                        }
                        if (IsADBReady)
                        {
                            if (ShowDialogs && await ShowDeviceDialog())
                            {
                                goto checkdevice;
                            }
                        }
                        else
                        {
                            ActionButtonEnable = true;
                            ActionButtonText = _loader.GetString("ADBMissing");
                        }
                    }
                }
                WaitProgressText = _loader.GetString("Finished");
            }
            else
            {
                ResetUI();
                ApkInfo ??= new ApkInfo();
                AppName = _loader.GetString("NoPackageWranning");
                FileSelectButtonText = _loader.GetString("Select");
                CancelOperationButtonText = _loader.GetString("Close");
                FileSelectVisibility = CancelOperationVisibility = Visibility.Visible;
                AppVersionVisibility = AppPublisherVisibility = AppCapabilitiesVisibility = Visibility.Collapsed;
            }
            IsInitialized = true;
        }

        private async Task<bool> ShowDeviceDialog()
        {
            if (IsOnlyWSA)
            {
                WaitProgressText = _loader.GetString("FindingWSA");
                if ((await Launcher.FindUriSchemeHandlersAsync("wsa"))?.Any() == true)
                {
                    WaitProgressText = _loader.GetString("FoundWSA");
                    ContentDialog dialog = new MarkdownDialog
                    {
                        XamlRoot = _page?.XamlRoot,
                        Title = _loader.GetString("HowToConnect"),
                        DefaultButton = ContentDialogButton.Close,
                        CloseButtonText = _loader.GetString("IKnow"),
                        PrimaryButtonText = _loader.GetString("StartWSA"),
                        FallbackContent = _loader.GetString("HowToConnectInfo"),
                        ContentInfo = new GitInfo("Paving-Base", "APK-Installer", "screenshots", "Documents/Tutorials/How%20To%20Connect%20WSA", "How%20To%20Connect%20WSA.md")
                    };
                    ProgressHelper.SetState(ProgressState.None, true);
                    ContentDialogResult result = await dialog.ShowAsync();
                    ProgressHelper.SetState(ProgressState.Indeterminate, true);
                    if (result == ContentDialogResult.Primary)
                    {
                        startwsa:
                        CancellationTokenSource TokenSource = new(TimeSpan.FromMinutes(5));
                        try
                        {
                            WaitProgressText = _loader.GetString("LaunchingWSA");
                            _ = await Launcher.LaunchUriAsync(new Uri("wsa://"));
                            bool IsWSARunning = false;
                            while (!IsWSARunning)
                            {
                                TokenSource.Token.ThrowIfCancellationRequested();
                                await Task.Run(() =>
                                {
                                    Process[] ps = Process.GetProcessesByName("vmmemWSA");
                                    IsWSARunning = ps != null && ps.Length > 0;
                                });
                            }
                            WaitProgressText = _loader.GetString("WaitingWSAStart");
                            while (!await CheckDevice(true))
                            {
                                TokenSource.Token.ThrowIfCancellationRequested();
                                await new AdbClient().ConnectAsync(new DnsEndPoint("127.0.0.1", 58526));
                                await Task.Delay(100);
                            }
                            WaitProgressText = _loader.GetString("WSARunning");
                            return true;
                        }
                        catch (OperationCanceledException ex)
                        {
                            SettingsHelper.LogManager.GetLogger(nameof(InstallViewModel)).Error(ex.ExceptionToMessage(), ex);
                            ContentDialog dialogs = new()
                            {
                                XamlRoot = _page?.XamlRoot,
                                Title = _loader.GetString("CannotConnectWSA"),
                                DefaultButton = ContentDialogButton.Close,
                                CloseButtonText = _loader.GetString("IKnow"),
                                PrimaryButtonText = _loader.GetString("Retry"),
                                Content = _loader.GetString("CannotConnectWSAInfo"),
                            };
                            ProgressHelper.SetState(ProgressState.None, true);
                            ContentDialogResult results = await dialogs.ShowAsync();
                            ProgressHelper.SetState(ProgressState.Indeterminate, true);
                            if (results == ContentDialogResult.Primary)
                            {
                                goto startwsa;
                            }
                        }
                        catch (Exception e)
                        {
                            SettingsHelper.LogManager.GetLogger(nameof(InstallViewModel)).Warn(e.ExceptionToMessage(), e);
                            ContentDialog dialogs = new()
                            {
                                XamlRoot = _page?.XamlRoot,
                                Title = _loader.GetString("CannotConnectWSA"),
                                DefaultButton = ContentDialogButton.Close,
                                CloseButtonText = _loader.GetString("IKnow"),
                                PrimaryButtonText = _loader.GetString("Retry"),
                                Content = new TextBlock
                                {
                                    Text = e.Message,
                                    IsTextSelectionEnabled = true
                                },
                            };
                            ProgressHelper.SetState(ProgressState.None, true);
                            ContentDialogResult results = await dialogs.ShowAsync();
                            ProgressHelper.SetState(ProgressState.Indeterminate, true);
                            if (results == ContentDialogResult.Primary)
                            {
                                goto startwsa;
                            }
                        }
                    }
                }
                else
                {
                    ContentDialog dialog = new()
                    {
                        XamlRoot = _page?.XamlRoot,
                        Title = _loader.GetString("NoDevice10"),
                        DefaultButton = ContentDialogButton.Primary,
                        CloseButtonText = _loader.GetString("IKnow"),
                        PrimaryButtonText = _loader.GetString("InstallWSA"),
                        SecondaryButtonText = _loader.GetString("GoToSetting"),
                        Content = _loader.GetString("NoDeviceInfo"),
                    };
                    ProgressHelper.SetState(ProgressState.None, true);
                    ContentDialogResult result = await dialog.ShowAsync();
                    ProgressHelper.SetState(ProgressState.Indeterminate, true);
                    if (result == ContentDialogResult.Primary)
                    {
                        _ = await Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp/?ProductId=9P3395VX91NR&mode=mini"));
                    }
                    else if (result == ContentDialogResult.Secondary)
                    {
                        UIHelper.Navigate(typeof(SettingsPage), null);
                    }
                }
            }
            else
            {
                ContentDialog dialog = new MarkdownDialog
                {
                    XamlRoot = _page?.XamlRoot,
                    Title = _loader.GetString("NoDevice"),
                    DefaultButton = ContentDialogButton.Close,
                    CloseButtonText = _loader.GetString("IKnow"),
                    PrimaryButtonText = _loader.GetString("GoToSetting"),
                    FallbackContent = _loader.GetString("NoDeviceInfo10"),
                    ContentInfo = new GitInfo("Paving-Base", "APK-Installer", "screenshots", "Documents/Tutorials/How%20To%20Connect%20Device", "How%20To%20Connect%20Device.md")
                };
                ProgressHelper.SetState(ProgressState.None, true);
                ContentDialogResult result = await dialog.ShowAsync();
                ProgressHelper.SetState(ProgressState.Indeterminate, true);
                if (result == ContentDialogResult.Primary)
                {
                    UIHelper.Navigate(typeof(SettingsPage), null);
                }
            }
            return false;
        }

        private async Task ReinitializeUI()
        {
            WaitProgressText = _loader.GetString("Loading");
            if ((!string.IsNullOrWhiteSpace(_path) || _url != null) && NetAPKExist)
            {
                if (!IsADBReady)
                {
                    ResetUI();
                    return;
                }
                checkdevice:
                if (await CheckDevice() && _device != null)
                {
                    await CheckAPK();
                }
                else
                {
                    if (ApkInfo == null)
                    {
                        await InitializeUI();
                    }
                    else
                    {
                        ResetUI();
                        ActionButtonEnable = false;
                        ActionButtonText = _loader.GetString("Install");
                        InfoMessage = _loader.GetString("WaitingDevice");
                        DeviceSelectButtonText = _loader.GetString("Devices");
                        AppName = string.Format(_loader.GetString("WaitingForInstallFormat"), AppLocaleName);
                        ActionVisibility = DeviceSelectVisibility = MessagesToUserVisibility = Visibility.Visible;

                        if (IsADBReady)
                        {
                            if (ShowDialogs && await ShowDeviceDialog())
                            {
                                goto checkdevice;
                            }
                        }
                        else
                        {
                            ActionButtonEnable = true;
                            ActionButtonText = _loader.GetString("ADBMissing");
                        }
                    }
                }
            }
        }

        private async Task CheckAPK()
        {
            ResetUI();
            if (_device != null)
            {
                try
                {
                    AdbClient client = new();
                    VersionInfo info = default;
                    if (ApkInfo != null && !ApkInfo.IsEmpty)
                    {
                        info = await client.GetPackageVersionAsync(_device, ApkInfo?.PackageName);
                    }
                    if (info == default)
                    {
                        ActionButtonText = _loader.GetString("Install");
                        AppName = string.Format(_loader.GetString("InstallFormat"), AppLocaleName);
                        ActionVisibility = Visibility.Visible;
                        LaunchWhenReadyVisibility = string.IsNullOrWhiteSpace(ApkInfo?.LaunchableActivity) ? Visibility.Collapsed : Visibility.Visible;
                    }
                    else if (info.VersionCode < int.Parse(ApkInfo?.VersionCode))
                    {
                        ActionButtonText = _loader.GetString("Update");
                        AppName = string.Format(_loader.GetString("UpdateFormat"), AppLocaleName);
                        ActionVisibility = Visibility.Visible;
                        LaunchWhenReadyVisibility = string.IsNullOrWhiteSpace(ApkInfo?.LaunchableActivity) ? Visibility.Collapsed : Visibility.Visible;
                    }
                    else
                    {
                        ActionButtonText = _loader.GetString("Reinstall");
                        SecondaryActionButtonText = _loader.GetString("Launch");
                        AppName = string.Format(_loader.GetString("ReinstallFormat"), AppLocaleName);
                        TextOutput = string.Format(_loader.GetString("ReinstallOutput"), AppLocaleName);
                        ActionVisibility = TextOutputVisibility = Visibility.Visible;
                        SecondaryActionVisibility = string.IsNullOrWhiteSpace(ApkInfo?.LaunchableActivity) ? Visibility.Collapsed : Visibility.Visible;
                    }
                    SDKInfo sdk = SDKInfo.GetInfo(await client.GetPropertyAsync(_device, "ro.build.version.sdk"));
                    if (sdk < ApkInfo.MinSDK)
                    {
                        ActionButtonEnable = false;
                        ContentDialog dialog = new()
                        {
                            XamlRoot = _page?.XamlRoot,
                            Content = string.Format(_loader.GetString("IncompatibleAppInfo"), ApkInfo?.MinSDK.ToString(), sdk.ToString()),
                            Title = _loader.GetString("IncompatibleApp"),
                            CloseButtonText = _loader.GetString("IKnow"),
                            DefaultButton = ContentDialogButton.Close
                        };
                        _ = dialog.ShowAsync();
                    }
                    return;
                }
                catch (Exception ex)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(InstallViewModel)).Error(ex.ExceptionToMessage(), ex);
                }
            }
            ActionButtonEnable = false;
            ActionButtonText = _loader.GetString("Install");
            InfoMessage = _loader.GetString("WaitingDevice");
            DeviceSelectButtonText = _loader.GetString("Devices");
            AppName = string.Format(_loader.GetString("WaitingForInstallFormat"), AppLocaleName);
            ActionVisibility = DeviceSelectVisibility = MessagesToUserVisibility = Visibility.Visible;
        }

        private void CheckOnlinePackage()
        {
            Regex[] UriRegex = [new(@":\?source=(.*)"), new(@"://(.*)")];
            string Uri = UriRegex[0].IsMatch(_url.ToString()) ? UriRegex[0].Match(_url.ToString()).Groups[1].Value : UriRegex[1].Match(_url.ToString()).Groups[1].Value;
            Uri Url = Uri.ValidateAndGetUri();
            if (Url != null)
            {
                _url = Url;
                AppName = _loader.GetString("OnlinePackage");
                DownloadButtonText = _loader.GetString("Download");
                CancelOperationButtonText = _loader.GetString("Close");
                DownloadVisibility = CancelOperationVisibility = Visibility.Visible;
                AppVersionVisibility = AppPublisherVisibility = AppCapabilitiesVisibility = Visibility.Collapsed;
                if (AutoGetNetAPK)
                {
                    LoadNetAPK();
                }
            }
            else
            {
                PackageError(_loader.GetString("InvalidURL"));
            }
        }

        public async void LoadNetAPK()
        {
            IsInstalling = true;
            DownloadVisibility = Visibility.Collapsed;
            try
            {
                await DownloadAPK();
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(InstallViewModel)).Error(ex.ExceptionToMessage(), ex);
                PackageError(ex.Message);
                IsInstalling = false;
                return;
            }

            try
            {
                ApkInfo = await Task.Run(() => { return AAPTool.Decompile(_path); });
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(InstallViewModel)).Error(ex.ExceptionToMessage(), ex);
                PackageError(ex.Message);
                IsInstalling = false;
                return;
            }

            if ((ApkInfo?.IsEmpty).GetValueOrDefault(true))
            {
                PackageError(_loader.GetString("InvalidPackage"));
            }
            else
            {
                if (await CheckDevice() && _device != null)
                {
                    await CheckAPK();
                }
                else
                {
                    ResetUI();
                    ActionButtonEnable = false;
                    ActionButtonText = _loader.GetString("Install");
                    InfoMessage = _loader.GetString("WaitingDevice");
                    DeviceSelectButtonText = _loader.GetString("Devices");
                    AppName = string.Format(_loader.GetString("WaitingForInstallFormat"), AppLocaleName);
                    ActionVisibility = DeviceSelectVisibility = MessagesToUserVisibility = Visibility.Visible;
                }
            }
            IsInstalling = false;
        }

        private async Task DownloadAPK()
        {
            if (_url != null)
            {
                if (!Directory.Exists(APKTemp[..APKTemp.LastIndexOf('\\')]))
                {
                    _ = Directory.CreateDirectory(APKTemp[..APKTemp.LastIndexOf('\\')]);
                }
                else if (Directory.Exists(APKTemp))
                {
                    Directory.Delete(APKTemp, true);
                }
                using (DownloadService downloader = new(DownloadHelper.Configuration))
                {
                    bool IsCompleted = false;
                    Exception exception = null;
                    long ReceivedBytesSize = 0;
                    long TotalBytesToReceive = 0;
                    double ProgressPercentage = 0;
                    double BytesPerSecondSpeed = 0;
                    void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
                    {
                        exception = e.Error;
                        IsCompleted = true;
                    }
                    void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
                    {
                        ReceivedBytesSize = e.ReceivedBytesSize;
                        ProgressPercentage = e.ProgressPercentage;
                        TotalBytesToReceive = e.TotalBytesToReceive;
                        BytesPerSecondSpeed = e.BytesPerSecondSpeed;
                    }
                    downloader.DownloadFileCompleted += OnDownloadFileCompleted;
                    downloader.DownloadProgressChanged += OnDownloadProgressChanged;
                    downloadapk:
                    ProgressText = _loader.GetString("WaitDownload");
                    _ = downloader.DownloadFileTaskAsync(_url.ToString(), APKTemp);
                    while (TotalBytesToReceive <= 0)
                    {
                        await Task.Delay(1);
                        if (IsCompleted)
                        {
                            goto downloadfinish;
                        }
                    }
                    AppxInstallBarIndeterminate = false;
                    ProgressHelper.SetState(ProgressState.Normal, true);
                    while (!IsCompleted)
                    {
                        ProgressHelper.SetValue(Convert.ToInt32(ReceivedBytesSize), Convert.ToInt32(TotalBytesToReceive), true);
                        ProgressText = $"{ProgressPercentage:N2}% {((double)BytesPerSecondSpeed).GetSizeString()}/s";
                        AppxInstallBarValue = ProgressPercentage;
                        await Task.Delay(1);
                    }
                    ProgressHelper.SetState(ProgressState.Indeterminate, true);
                    ProgressText = _loader.GetString("Loading");
                    AppxInstallBarIndeterminate = true;
                    AppxInstallBarValue = 0;
                    downloadfinish:
                    if (exception != null)
                    {
                        ProgressHelper.SetState(ProgressState.Error, true);
                        ContentDialog dialog = new()
                        {
                            XamlRoot = _page?.XamlRoot,
                            Content = exception.Message,
                            Title = _loader.GetString("DownloadFailed"),
                            PrimaryButtonText = _loader.GetString("Retry"),
                            CloseButtonText = _loader.GetString("Cancel"),
                            DefaultButton = ContentDialogButton.Primary
                        };
                        ContentDialogResult result = await dialog.ShowAsync();
                        ProgressHelper.SetState(ProgressState.Indeterminate, true);
                        if (result == ContentDialogResult.Primary)
                        {
                            goto downloadapk;
                        }
                        else
                        {
                            SendResults(new Exception($"APK {_loader.GetString("DownloadFailed")}"));
                            Application.Current.Exit();
                            return;
                        }
                    }
                    downloader.DownloadProgressChanged -= OnDownloadProgressChanged;
                    downloader.DownloadFileCompleted -= OnDownloadFileCompleted;
                }
            }
        }

        private void ResetUI()
        {
            ActionVisibility =
            SecondaryActionVisibility =
            FileSelectVisibility =
            DownloadVisibility =
            DeviceSelectVisibility =
            CancelOperationVisibility =
            TextOutputVisibility =
            InstallOutputVisibility =
            LaunchWhenReadyVisibility =
            MessagesToUserVisibility = Visibility.Collapsed;
            AppVersionVisibility =
            AppPublisherVisibility =
            AppCapabilitiesVisibility = Visibility.Visible;
            AppxInstallBarIndeterminate =
            ActionButtonEnable =
            SecondaryActionButtonEnable =
            FileSelectButtonEnable =
            DownloadButtonEnable =
            DeviceSelectButtonEnable =
            CancelOperationButtonEnable = true;
        }

        private void PackageError(string message)
        {
            ResetUI();
            TextOutput = message;
            ApkInfo ??= new ApkInfo();
            ProgressHelper.SetState(ProgressState.Error, true);
            AppName = _loader.GetString("CannotOpenPackage");
            TextOutputVisibility = InstallOutputVisibility = Visibility.Visible;
            AppVersionVisibility = AppPublisherVisibility = AppCapabilitiesVisibility = Visibility.Collapsed;
        }

        private void OnDeviceListChanged(object sender, DeviceDataNotifyEventArgs e)
        {
            if (IsInitialized && !IsInstalling)
            {
                _page?.DispatcherQueue.EnqueueAsync(async () =>
                {
                    if (await CheckDevice() && _device != null)
                    {
                        await CheckAPK();
                    }
                });
            }
        }

        private async Task<bool> CheckDevice(bool forces = false)
        {
            if (!IsADBReady) { return false; }
            AdbClient client = new();
            IEnumerable<DeviceData> devices = await client.GetDevicesAsync();
            ConsoleOutputReceiver receiver = new();
            if (!devices.Any()) { return false; }
            foreach (DeviceData device in devices)
            {
                if (device == null || forces ? device.State == DeviceState.Offline : device.State != DeviceState.Online) { continue; }
                if (IsOnlyWSA)
                {
                    await client.ExecuteRemoteCommandAsync("getprop ro.boot.hardware", device, receiver);
                    if (receiver.ToString().Contains("windows"))
                    {
                        _device = device;
                        return true;
                    }
                }
                else
                {
                    DeviceData data = SettingsHelper.Get<DeviceData>(SettingsHelper.DefaultDevice);
                    if (data != null && data.Name == device.Name && data.Model == device.Model && data.Product == device.Product)
                    {
                        SettingsHelper.Set(SettingsHelper.DefaultDevice, device);
                        _device = device;
                        return true;
                    }
                }
            }
            _device = null;
            return false;
        }

        public void OpenAPP() => _ = new AdbClient().StartAppAsync(_device, ApkInfo?.PackageName);

        public async void InstallAPP()
        {
            try
            {
                AdbClient client = new();
                VersionInfo info = default;
                if (ApkInfo != null && !ApkInfo.IsEmpty)
                {
                    info = await client.GetPackageVersionAsync(_device, ApkInfo?.PackageName);
                }
                if (info != default && info.VersionCode >= int.Parse(ApkInfo?.VersionCode))
                {
                    ContentDialog dialog = new()
                    {
                        XamlRoot = _page?.XamlRoot,
                        Content = string.Format(_loader.GetString("HasNewerVersionInfo"), info.VersionName, ApkInfo?.VersionName),
                        Title = _loader.GetString("HasNewerVersion"),
                        PrimaryButtonText = _loader.GetString("Reinstall"),
                        CloseButtonText = _loader.GetString("Cancel"),
                        DefaultButton = ContentDialogButton.Close
                    };
                    ContentDialogResult result = await dialog.ShowAsync();
                    if (result != ContentDialogResult.Primary) { return; }
                }
                IsInstalling = true;
                AppxInstallBarIndeterminate = true;
                ProgressText = _loader.GetString("Installing");
                CancelOperationButtonText = _loader.GetString("Cancel");
                CancelOperationVisibility = Visibility.Visible;
                ActionVisibility = SecondaryActionVisibility = TextOutputVisibility = InstallOutputVisibility = Visibility.Collapsed;
                LaunchWhenReadyVisibility = string.IsNullOrWhiteSpace(ApkInfo?.LaunchableActivity) ? Visibility.Collapsed : Visibility.Visible;
                switch (ApkInfo)
                {
                    case { IsSplit: true } when IsUploadAPK:
                        await client.InstallMultiplePackageAsync(_device, [ApkInfo.FullPath], ApkInfo.PackageName, OnInstallProgressChanged, default, "-r", "-t");
                        break;
                    case { IsSplit: true }:
                        await using (FileStream apk = File.Open(ApkInfo.FullPath, FileMode.Open, FileAccess.Read))
                        {
                            await client.InstallMultipleAsync(_device, [apk], ApkInfo.PackageName, OnInstallProgressChanged, default, "-r", "-t");
                        }
                        break;
                    case { IsBundle: true } when IsUploadAPK:
                        IEnumerable<string> strings = ApkInfo.SplitApks?.Select(x => x.FullPath);
                        await client.InstallMultiplePackageAsync(_device, ApkInfo.FullPath, strings, OnInstallProgressChanged, default, "-r", "-t");
                        break;
                    case { IsBundle: true }:
                        await using (FileStream apk = File.Open(ApkInfo.FullPath, FileMode.Open, FileAccess.Read))
                        {
                            FileStream[] splits = [.. ApkInfo.SplitApks.Select(x => File.Open(x.FullPath, FileMode.Open, FileAccess.Read))];
                            await client.InstallMultipleAsync(_device, apk, splits, OnInstallProgressChanged, default, "-r", "-t");
                            await Task.WhenAll(splits.Select(x => x.DisposeAsync().AsTask()));
                        }
                        break;
                    case not null when IsUploadAPK:
                        await client.InstallPackageAsync(_device, ApkInfo.FullPath, OnInstallProgressChanged, default, "-r", "-t");
                        break;
                    case not null:
                        await using (FileStream apk = File.Open(ApkInfo.FullPath, FileMode.Open, FileAccess.Read))
                        {
                            await client.InstallAsync(_device, apk, OnInstallProgressChanged, default, "-r", "-t");
                        }
                        break;
                }
                AppName = string.Format(_loader.GetString("InstalledFormat"), AppLocaleName);
                if (IsOpenApp && !string.IsNullOrWhiteSpace(ApkInfo?.LaunchableActivity))
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1000);// 据说如果安装完直接启动会崩溃。。。
                        OpenAPP();
                        if (IsCloseAPP)
                        {
                            await Task.Delay(5000);
                            _page?.DispatcherQueue.TryEnqueue(() => Application.Current.Exit());
                        }
                    });
                }
                SendResults();
                IsInstalling = false;
                AppxInstallBarValue = 0;
                AppxInstallBarIndeterminate = true;
                ActionButtonText = _loader.GetString("Reinstall");
                SecondaryActionButtonText = _loader.GetString("Launch");
                ActionVisibility = Visibility.Visible;
                CancelOperationVisibility = LaunchWhenReadyVisibility = Visibility.Collapsed;
                SecondaryActionVisibility = string.IsNullOrWhiteSpace(ApkInfo?.LaunchableActivity) ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception ex)
            {
                SendResults(ex);
                IsInstalling = false;
                TextOutput = ex.Message;
                TextOutputVisibility = InstallOutputVisibility = Visibility.Visible;
                SettingsHelper.LogManager.GetLogger(nameof(InstallViewModel)).Error(ex.ExceptionToMessage(), ex);
                ActionVisibility = SecondaryActionVisibility = CancelOperationVisibility = LaunchWhenReadyVisibility = Visibility.Collapsed;
            }

            _page.CancelFlyout.Hide();

            void OnInstallProgressChanged(InstallProgressEventArgs e)
            {
                switch (e.State)
                {
                    case PackageInstallProgressState.Uploading:
                        AppxInstallBarIndeterminate = false;
                        AppxInstallBarValue = e.UploadProgress;
                        ProgressHelper.SetState(ProgressState.Normal, true);
                        ProgressHelper.SetValue(Convert.ToInt32(e.UploadProgress), 100, true);
                        ProgressText = string.Format(_loader.GetString("UploadingFormat"), $"{e.UploadProgress:N0}%", e.PackageFinished, e.PackageRequired);
                        break;
                    case PackageInstallProgressState.CreateSession:
                        AppxInstallBarIndeterminate = true;
                        ProgressHelper.SetState(ProgressState.Indeterminate, true);
                        ProgressText = _loader.GetString("CreateSession");
                        break;
                    case PackageInstallProgressState.WriteSession:
                        double value = e.PackageFinished * 100 / Math.Max(e.PackageRequired, 1);
                        AppxInstallBarValue = value;
                        AppxInstallBarIndeterminate = false;
                        ProgressHelper.SetState(ProgressState.Normal, true);
                        ProgressHelper.SetValue(Convert.ToInt32(value), 100, true);
                        ProgressText = string.Format(_loader.GetString("WriteSessionFormat"), e.PackageFinished, e.PackageRequired);
                        break;
                    case PackageInstallProgressState.Installing:
                        AppxInstallBarIndeterminate = true;
                        ProgressHelper.SetState(ProgressState.Indeterminate, true);
                        ProgressText = _loader.GetString("Installing");
                        break;
                    case PackageInstallProgressState.PostInstall:
                        value = e.PackageFinished * 100 / Math.Max(e.PackageRequired, 1);
                        AppxInstallBarValue = value;
                        AppxInstallBarIndeterminate = false;
                        ProgressHelper.SetState(ProgressState.Normal, true);
                        ProgressHelper.SetValue(Convert.ToInt32(value), 100, true);
                        ProgressText = string.Format(_loader.GetString("PostInstallFormat"), e.PackageFinished, e.PackageRequired);
                        break;
                    case PackageInstallProgressState.Finished:
                        AppxInstallBarValue = 100;
                        AppxInstallBarIndeterminate = false;
                        ProgressHelper.SetState(ProgressState.Normal, true);
                        ProgressHelper.SetValue(100, 100, true);
                        ProgressText = _loader.GetString("Finished");
                        break;
                    default:
                        break;
                }
            }
        }

        public async void OpenAPK(string path)
        {
            if (path != null)
            {
                _path = path;
                await _page?.DispatcherQueue.EnqueueAsync(async () => await Refresh());
            }
        }

        public async void OpenAPK()
        {
            FileOpenPicker FileOpen = new();
            FileOpen.FileTypeFilter.Add(".apk");
            FileOpen.FileTypeFilter.Add(".apks");
            FileOpen.FileTypeFilter.Add(".apkm");
            FileOpen.FileTypeFilter.Add(".xapk");
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
                _path = file.Path;
                await Refresh();
            }
        }

        public async void OpenAPK(DataPackageView data)
        {
            WaitProgressText = _loader.GetString("CheckingPath");
            IsInitialized = false;

            await Task.Run(async () =>
            {
                if (data.Contains(StandardDataFormats.StorageItems))
                {
                    IReadOnlyList<IStorageItem> items = await data.GetStorageItemsAsync();
                    if (items.Count == 1)
                    {
                        IStorageItem storageItem = items[0];
                        await OpenPath(storageItem);
                        return;
                    }
                    else if (items.Count >= 1)
                    {
                        await CreateAPKS(items);
                        return;
                    }
                }
                else if (data.Contains(StandardDataFormats.Uri))
                {
                    string path = (await data.GetUriAsync()).ToString();
                    if (Directory.Exists(path))
                    {
                        StorageFolder storageItem = await StorageFolder.GetFolderFromPathAsync(path);
                        await OpenPath(storageItem);
                        return;
                    }
                    else if (File.Exists(path))
                    {
                        StorageFile storageItem = await StorageFile.GetFileFromPathAsync(path);
                        await OpenPath(storageItem);
                        return;
                    }
                }
                else if (data.Contains(StandardDataFormats.Text))
                {
                    string path = await data.GetTextAsync();
                    if (Directory.Exists(path))
                    {
                        StorageFolder storageItem = await StorageFolder.GetFolderFromPathAsync(path);
                        await OpenPath(storageItem);
                        return;
                    }
                    else if (File.Exists(path))
                    {
                        StorageFile storageItem = await StorageFile.GetFileFromPathAsync(path);
                        await OpenPath(storageItem);
                        return;
                    }
                }
                IsInitialized = true;
            });

            async Task OpenPath(IStorageItem storageItem)
            {
                if (storageItem != null)
                {
                    if (storageItem is StorageFolder folder)
                    {
                        IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                        await CreateAPKS(files);
                        return;
                    }
                    else
                    {
                        if (storageItem.Name.ToLower().EndsWith(".apk"))
                        {
                            OpenAPK(storageItem.Path);
                            return;
                        }
                        try
                        {
                            using (IArchive archive = ArchiveFactory.Open(storageItem.Path))
                            {
                                foreach (IArchiveEntry entry in archive.Entries.Where(x => !x.Key.Contains('/')))
                                {
                                    if (entry.Key.ToLower().EndsWith(".apk"))
                                    {
                                        OpenAPK(storageItem.Path);
                                        return;
                                    }
                                }
                            }
                            IsInitialized = true;
                        }
                        catch (Exception ex) { SettingsHelper.LogManager.GetLogger(nameof(InstallViewModel)).Warn(ex.ExceptionToMessage(), ex); }
                    }
                }
                IsInitialized = true;
            }

            async Task CreateAPKS(IReadOnlyList<IStorageItem> items)
            {
                await ThreadSwitcher.ResumeBackgroundAsync();
                List<string> apks = [];
                foreach (IStorageItem item in items)
                {
                    if (item != null)
                    {
                        if (item.Name.ToLower().EndsWith(".apk"))
                        {
                            apks.Add(item.Path);
                            continue;
                        }
                        try
                        {
                            using (IArchive archive = ArchiveFactory.Open(item.Path))
                            {
                                foreach (IArchiveEntry entry in archive.Entries.Where(x => !x.Key.Contains('/')))
                                {
                                    if (entry.Key.ToLower().EndsWith(".apk"))
                                    {
                                        OpenAPK(item.Path);
                                        return;
                                    }
                                }
                            }
                            apks.Add(item.Path);
                            continue;
                        }
                        catch (Exception ex)
                        {
                            SettingsHelper.LogManager.GetLogger(nameof(InstallViewModel)).Warn(ex.ExceptionToMessage(), ex);
                            continue;
                        }
                    }
                }

                if (apks.Count == 1)
                {
                    OpenAPK(apks.FirstOrDefault());
                    return;
                }
                else if (apks.Count >= 1)
                {
                    IEnumerable<string> apklist = apks.Where(x => x.EndsWith(".apk"));
                    if (apklist.Any())
                    {
                        string temp = Path.Combine(CachesHelper.TempPath, $"TempSplitAPK.apks");

                        if (!Directory.Exists(temp[..temp.LastIndexOf('\\')]))
                        {
                            _ = Directory.CreateDirectory(temp[..temp.LastIndexOf('\\')]);
                        }
                        else if (Directory.Exists(temp))
                        {
                            Directory.Delete(temp, true);
                        }

                        if (File.Exists(temp))
                        {
                            File.Delete(temp);
                        }

                        using (FileStream zip = File.OpenWrite(temp))
                        {
                            using IWriter zipWriter = WriterFactory.Open(zip, ArchiveType.Zip, CompressionType.Deflate);
                            foreach (string apk in apks.Where(x => x.EndsWith(".apk")))
                            {
                                zipWriter.Write(Path.GetFileName(apk), apk);
                            }
                        }

                        OpenAPK(temp);
                        return;
                    }
                    else
                    {
                        IEnumerable<string> apkslist = apks.Where(x => !x.EndsWith(".apk"));
                        if (apkslist.Count() == 1)
                        {
                            OpenAPK(apkslist.FirstOrDefault());
                            return;
                        }
                    }
                }

                IsInitialized = true;
            }
        }

        private void SendResults(Exception exception = null)
        {
            if (_operation == null) { return; }
            ValueSet results = new()
            {
                ["Result"] = exception != null,
                ["Exception"] = exception
            };
            _operation.ReportCompleted(results);
        }

        public void CloseAPP()
        {
            SendResults(new Exception($"{_loader.GetString("Install")} {_loader.GetString("Cancel")}"));
            Application.Current.Exit();
        }
    }
}