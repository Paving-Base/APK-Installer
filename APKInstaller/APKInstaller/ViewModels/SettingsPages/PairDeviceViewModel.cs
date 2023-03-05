using AdvancedSharpAdbClient;
using APKInstaller.Controls.Dialogs;
using APKInstaller.Helpers;
using APKInstaller.Models;
using APKInstaller.Pages.SettingsPages;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Resources;
using Zeroconf;
using Zeroconf.Interfaces;

namespace APKInstaller.ViewModels.SettingsPages
{
    public class PairDeviceViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly PairDevicePage _page;
        //private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("PairDeviceDialog");

        private static string ADBPath => SettingsHelper.Get<string>(SettingsHelper.ADBPath);

        public string Code { get; set; } = string.Empty;

        public ResolverListener ConnectListener;

        public readonly ObservableCollection<MDNSDeviceData> DeviceList = new();

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

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public PairDeviceViewModel(PairDevicePage Page) => _page = Page;

        public void InitializeConnectListener()
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
                MonitorHelper.Monitor.DeviceChanged += OnDeviceChanged;
                ConnectedList = new AdbClient().GetDevices().Where(x => x.State == DeviceState.Online).ToList();
            }
        }

        public async Task ConnectWithPairingCode(MDNSDeviceData deviceData)
        {
            deviceData.ConnectingDevice = true;
            IAdbServer ADBServer = AdbServer.Instance;
            if (!ADBServer.GetStatus().IsRunning)
            {
                try
                {
                    _ = await Task.Run(() => ADBServer.StartServer(ADBPath, restartServerIfNewer: false));
                    MonitorHelper.Monitor.DeviceChanged += OnDeviceChanged;
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
            string code = Code;
            if (!string.IsNullOrWhiteSpace(code) && deviceData != null)
            {
                try
                {
                    AdbClient client = new();
                    string pair = await client.PairAsync(deviceData.Address, deviceData.Port, code);
                    if (pair.ToLowerInvariant().StartsWith("successfully"))
                    {
                        ConnectInfoSeverity = InfoBarSeverity.Success;
                        ConnectInfoTitle = pair;
                        ConnectInfoIsOpen = true;
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
                        if (connect.ToLowerInvariant().StartsWith("connected to"))
                        {
                            ConnectInfoSeverity = InfoBarSeverity.Success;
                            ConnectInfoTitle = pair;
                            ConnectInfoIsOpen = true;
                        }
                    }
                    else if (pair.ToLowerInvariant().StartsWith("failed:"))
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
            }
            deviceData.ConnectingDevice = false;
        }

        private void ConnectListener_ServiceFound(object sender, IZeroconfHost e)
        {
            _ = _page.DispatcherQueue.EnqueueAsync(() =>
            {
                bool add = true;
                MDNSDeviceData deviceData = new(e);
                foreach (MDNSDeviceData data in DeviceList)
                {
                    if (data.Address == deviceData.Address)
                    {
                        if (data.Port == deviceData.Port)
                        {
                            add = false;
                        }
                        else
                        {
                            _ = DeviceList.Remove(data);
                        }
                    }
                }
                if (add) { DeviceList.Add(deviceData); }
            });
        }

        private void ConnectListener_ServiceLost(object sender, IZeroconfHost e)
        {
            _ = _page.DispatcherQueue.EnqueueAsync(() =>
            {
                foreach (MDNSDeviceData data in DeviceList)
                {
                    if (data.Name == e.DisplayName)
                    {
                        _ = DeviceList.Remove(data);
                    }
                }
            });
        }

        public void OnDeviceChanged(object sender, DeviceDataEventArgs e) => _ = (_page?.DispatcherQueue.EnqueueAsync(() => ConnectedList = new AdbClient().GetDevices().Where(x => x.State == DeviceState.Online).ToList()));

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
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
                if (AdbServer.Instance.GetStatus().IsRunning)
                {
                    MonitorHelper.Monitor.DeviceChanged -= OnDeviceChanged;
                }
                if (!SettingsHelper.Get<bool>(SettingsHelper.ScanPairedDevice))
                {
                    MonitorHelper.DisposeConnectListener();
                }
            }
        }
    }
}
