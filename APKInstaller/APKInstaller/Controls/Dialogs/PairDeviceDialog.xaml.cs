using Zeroconf;
using Zeroconf.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace APKInstaller.Controls.Dialogs
{
    public sealed partial class PairDeviceDialog : ContentDialog, INotifyPropertyChanged
    {
        private ResolverListener ConnectListener;

        internal ObservableCollection<IZeroconfHost> DeviceList = new();

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public PairDeviceDialog() => InitializeComponent();

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            ConnectListener = ZeroconfResolver.CreateListener("_adb-tls-pairing._tcp.local.");
            ConnectListener.ServiceFound += ConnectListener_ServiceFound;
            ConnectListener.ServiceLost += ConnectListener_ServiceLost;
        }

        private void ContentDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            ConnectListener.ServiceLost -= ConnectListener_ServiceLost;
            ConnectListener.ServiceFound -= ConnectListener_ServiceFound;
            ConnectListener.Dispose();
            DeviceList.Clear();
        }

        private void ConnectListener_ServiceFound(object sender, IZeroconfHost e) => DeviceList.Add(e);

        private void ConnectListener_ServiceLost(object sender, IZeroconfHost e) => DeviceList.Remove(e);
    }
}
