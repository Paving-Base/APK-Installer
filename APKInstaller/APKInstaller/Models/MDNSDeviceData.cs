using APKInstaller.Helpers;
using System.ComponentModel;
using System.Linq;
using Zeroconf.Interfaces;

namespace APKInstaller.Models
{
    public partial class MDNSDeviceData : INotifyPropertyChanged
    {
        public string Name { get; private set; }
        public string Address { get; private set; }
        public int Port { get; private set; }

        public string Host => $"{Address}:{Host}";

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

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public MDNSDeviceData(string name, string address, int port)
        {
            Name = name;
            Address = address;
            Port = port;
        }

        public MDNSDeviceData(IZeroconfHost host) : this(host.DisplayName, host.IPAddress, host.Services.FirstOrDefault().Value.Port) { }

        public override string ToString() => $"{Name} - {Host}";
    }
}
