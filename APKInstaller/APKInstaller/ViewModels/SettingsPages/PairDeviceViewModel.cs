using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.Models;
using APKInstaller.Helpers;
using APKInstaller.Models;
using APKInstaller.Pages.SettingsPages;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Zeroconf;
using Zeroconf.Interfaces;

namespace APKInstaller.ViewModels.SettingsPages
{
    public partial class PairDeviceViewModel : INotifyPropertyChanged, IDisposable
    {
        private string ssid;
        private string password;
        private readonly PairDevicePage _page;
        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("PairDevicePage");

        private static string ADBPath => SettingsHelper.Get<string>(SettingsHelper.ADBPath);

        public ResolverListener ConnectListener;

        public string DeviceListFormat => _loader.GetString("DeviceListFormat");
        public string ConnectedListFormat => _loader.GetString("ConnectedListFormat");

        public readonly ObservableCollection<MDNSDeviceData> DeviceList = [];

        private string _code = string.Empty;
        public string Code
        {
            get => _code;
            set
            {
                if (_code != value)
                {
                    _code = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _IPAddress = string.Empty;
        public string IPAddress
        {
            get => _IPAddress;
            set
            {
                if (_IPAddress != value)
                {
                    _IPAddress = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private List<DeviceData> _connectedList;
        public List<DeviceData> ConnectedList
        {
            get => _connectedList;
            set
            {
                if (_connectedList != value)
                {
                    _connectedList = value;
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

        private string _connectLogText;
        public string ConnectLogText
        {
            get => _connectLogText;
            set
            {
                if (_connectLogText != value)
                {
                    _connectLogText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _QRCodeText;
        public string QRCodeText
        {
            get => _QRCodeText;
            set
            {
                if (_QRCodeText != value)
                {
                    _QRCodeText = value;
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

        public PairDeviceViewModel(PairDevicePage Page) => _page = Page;

        public async void InitializeConnectListener()
        {
            if (!SettingsHelper.Get<bool>(SettingsHelper.ScanPairedDevice))
            {
                MonitorHelper.InitializeConnectListener();
                _ = MonitorHelper.ConnectPairedDevice();
            }
            if (ConnectListener == null)
            {
                ConnectListener = ZeroconfResolver.CreateListener("_adb-tls-pairing._tcp.local.");
                ConnectListener.ServiceFound += ConnectListener_ServiceFound;
                ConnectListener.ServiceLost += ConnectListener_ServiceLost;
            }
            if (AdbServer.Instance.GetStatus().IsRunning)
            {
                MonitorHelper.Monitor.DeviceListChanged += OnDeviceListChanged;
                ConnectedList = [.. (await new AdbClient().GetDevicesAsync()).Where(x => x.State != DeviceState.Offline)];
            }
        }

        public Task ConnectWithPairingCode(MDNSDeviceData deviceData) => ConnectWithPairingCode(deviceData, Code);

        public async Task ConnectWithPairingCode(MDNSDeviceData deviceData, string code)
        {
            if (!string.IsNullOrWhiteSpace(code) && deviceData != null)
            {
                deviceData.ConnectingDevice = true;
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
                        SettingsHelper.LogManager.GetLogger(nameof(PairDeviceViewModel)).Warn(ex.ExceptionToMessage(), ex);
                        ConnectInfoSeverity = InfoBarSeverity.Warning;
                        ConnectInfoTitle = ResourceLoader.GetForViewIndependentUse("InstallPage").GetString("ADBMissing");
                        ConnectInfoIsOpen = true;
                        deviceData.ConnectingDevice = false;
                        return;
                    }
                }
                try
                {
                    ConnectLogText = _loader.GetString("PairingLogText");
                    AdbClient client = new();
                    string pair = await client.PairAsync(deviceData.Address, deviceData.Port, code);
                    if (pair.StartsWith("successfully", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ConnectInfoSeverity = InfoBarSeverity.Success;
                        ConnectInfoTitle = pair;
                        ConnectInfoIsOpen = true;
                        ConnectLogText = _loader.GetString("ConnectingLogText");
                        string connect = await Task.Run(async () =>
                        {
                            using CancellationTokenSource tokenSource = new(TimeSpan.FromSeconds(10));
                            while (!tokenSource.Token.IsCancellationRequested)
                            {
                                IReadOnlyList<IZeroconfHost> hosts = await ZeroconfResolver.ResolveAsync("_adb-tls-connect._tcp.local.");
                                IZeroconfHost host = hosts.FirstOrDefault((x) => x.IPAddress == deviceData.Address);
                                if (host != null)
                                {
                                    string connect = await client.ConnectAsync(host.IPAddress, host.Services.FirstOrDefault().Value.Port);
                                    return connect;
                                }
                            }
                            return string.Empty;
                        });
                        if (connect.StartsWith("connected to", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ConnectInfoSeverity = InfoBarSeverity.Success;
                            ConnectInfoTitle = pair;
                            ConnectInfoIsOpen = true;
                            ConnectLogText = _loader.GetString("ConnectedLogText");
                        }
                    }
                    else if (pair.StartsWith("failed:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ConnectInfoSeverity = InfoBarSeverity.Error;
                        ConnectInfoTitle = pair[8..];
                        ConnectInfoIsOpen = true;
                    }
                    else if (!string.IsNullOrWhiteSpace(pair))
                    {
                        ConnectInfoSeverity = InfoBarSeverity.Warning;
                        ConnectInfoTitle = pair;
                        ConnectInfoIsOpen = true;
                    }
                }
                catch (Exception ex)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(PairDeviceViewModel)).Warn(ex.ExceptionToMessage(), ex);
                    ConnectInfoSeverity = InfoBarSeverity.Error;
                    ConnectInfoTitle = ex.Message;
                    ConnectInfoIsOpen = true;
                }
                deviceData.ConnectingDevice = false;
            }
        }

        public async Task ConnectWithPairingCode(string host, string code)
        {
            if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(code))
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
                        SettingsHelper.LogManager.GetLogger(nameof(PairDeviceViewModel)).Warn(ex.ExceptionToMessage(), ex);
                        ConnectInfoSeverity = InfoBarSeverity.Warning;
                        ConnectInfoTitle = ResourceLoader.GetForViewIndependentUse("InstallPage").GetString("ADBMissing");
                        ConnectInfoIsOpen = true;
                        ConnectingDevice = false;
                        return;
                    }
                }
                try
                {
                    ConnectLogText = _loader.GetString("PairingLogText");
                    AdbClient client = new();
                    string pair = await client.PairAsync(host, code);
                    if (pair.StartsWith("successfully", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ConnectInfoSeverity = InfoBarSeverity.Success;
                        ConnectInfoTitle = pair;
                        ConnectInfoIsOpen = true;
                        ConnectLogText = _loader.GetString("ConnectingLogText");
                        string connect = await Task.Run(async () =>
                        {
                            using CancellationTokenSource tokenSource = new(TimeSpan.FromSeconds(10));
                            while (!tokenSource.Token.IsCancellationRequested)
                            {
                                IReadOnlyList<IZeroconfHost> hosts = await ZeroconfResolver.ResolveAsync("_adb-tls-connect._tcp.local.");
                                IZeroconfHost _host = hosts.FirstOrDefault((x) => host.StartsWith(x.IPAddress));
                                if (_host != null)
                                {
                                    string connect = await client.ConnectAsync(_host.IPAddress, _host.Services.FirstOrDefault().Value.Port);
                                    return connect;
                                }
                            }
                            return string.Empty;
                        });
                        if (connect.StartsWith("connected to", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ConnectInfoSeverity = InfoBarSeverity.Success;
                            ConnectInfoTitle = pair;
                            ConnectInfoIsOpen = true;
                            ConnectLogText = _loader.GetString("ConnectedLogText");
                        }
                    }
                    else if (pair.StartsWith("failed:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ConnectInfoSeverity = InfoBarSeverity.Error;
                        ConnectInfoTitle = pair[8..];
                        ConnectInfoIsOpen = true;
                    }
                    else if (!string.IsNullOrWhiteSpace(pair))
                    {
                        ConnectInfoSeverity = InfoBarSeverity.Warning;
                        ConnectInfoTitle = pair;
                        ConnectInfoIsOpen = true;
                    }
                }
                catch (Exception ex)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(PairDeviceViewModel)).Warn(ex.ExceptionToMessage(), ex);
                    ConnectInfoSeverity = InfoBarSeverity.Error;
                    ConnectInfoTitle = ex.Message;
                    ConnectInfoIsOpen = true;
                }
                ConnectingDevice = false;
            }
        }

        public async Task ConnectWithPairingCode(string host)
        {
            if (!string.IsNullOrWhiteSpace(host))
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
                        SettingsHelper.LogManager.GetLogger(nameof(PairDeviceViewModel)).Warn(ex.ExceptionToMessage(), ex);
                        ConnectInfoSeverity = InfoBarSeverity.Warning;
                        ConnectInfoTitle = ResourceLoader.GetForViewIndependentUse("InstallPage").GetString("ADBMissing");
                        ConnectInfoIsOpen = true;
                        ConnectingDevice = false;
                        return;
                    }
                }
                try
                {
                    string results = (await new AdbClient().ConnectAsync(host)).TrimStart();
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
                    SettingsHelper.LogManager.GetLogger(nameof(PairDeviceViewModel)).Warn(ex.ExceptionToMessage(), ex);
                    ConnectInfoSeverity = InfoBarSeverity.Error;
                    ConnectInfoTitle = ex.Message;
                    ConnectInfoIsOpen = true;
                }
                ConnectingDevice = false;
            }
        }

        public void InitializeQRScan()
        {
            ssid = $"APKInstaller-{new Random().NextInt64(9999999999)}-4v4sx1";
            password = new Random().Next(999999).ToString();
            QRCodeText = $"WIFI:T:ADB;S:{ssid};P:{password};;";
            ConnectListener.ServiceFound += OnServiceFound;
        }

        public void DisposeQRScan()
        {
            QRCodeText = null;
            ConnectListener.ServiceFound -= OnServiceFound;
        }

        private async void OnServiceFound(object sender, IZeroconfHost e)
        {
            MDNSDeviceData deviceData = new(e);
            if (e.DisplayName == ssid)
            {
                ConnectListener.ServiceFound -= OnServiceFound;
                ConnectingDevice = true;
                await ConnectWithPairingCode(deviceData, password);
                ConnectingDevice = false;
                _ = _page.DispatcherQueue.EnqueueAsync(_page.HideQRScanFlyout);
            }
        }

        private void ConnectListener_ServiceFound(object sender, IZeroconfHost e)
        {
            bool add = true;
            MDNSDeviceData deviceData = new(e);
            foreach (MDNSDeviceData data in DeviceList)
            {
                if (data.Address == deviceData.Address)
                {
                    _ = _page.DispatcherQueue.EnqueueAsync(() => DeviceList.Remove(data));
                }
            }
            if (add) { _page.DispatcherQueue.EnqueueAsync(() => DeviceList.Add(deviceData)); }
        }

        private void ConnectListener_ServiceLost(object sender, IZeroconfHost e)
        {
            foreach (MDNSDeviceData data in DeviceList)
            {
                if (data.Name == e.DisplayName)
                {
                    _ = _page.DispatcherQueue.EnqueueAsync(() => DeviceList.Remove(data));
                }
            }
        }

        public async void OnDeviceListChanged(object sender, DeviceDataNotifyEventArgs e) => ConnectedList = [.. (await new AdbClient().GetDevicesAsync()).Where(x => x.State != DeviceState.Offline)];

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ConnectListener != null)
                {
                    ConnectListener.ServiceLost -= ConnectListener_ServiceLost;
                    ConnectListener.ServiceFound -= ConnectListener_ServiceFound;
                    ConnectListener.Dispose();
                }
                DeviceList.Clear();
                if ((await AdbServer.Instance.GetStatusAsync(CancellationToken.None)).IsRunning)
                {
                    MonitorHelper.Monitor.DeviceListChanged -= OnDeviceListChanged;
                }
                if (!SettingsHelper.Get<bool>(SettingsHelper.ScanPairedDevice))
                {
                    MonitorHelper.DisposeConnectListener();
                }
            }
        }
    }
}
