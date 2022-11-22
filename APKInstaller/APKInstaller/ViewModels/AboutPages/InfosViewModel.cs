using AAPTForNet.Models;
using APKInstaller.Pages;
using APKInstaller.Pages.AboutPages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace APKInstaller.ViewModels.AboutPages
{
    public class InfosViewModel : INotifyPropertyChanged
    {
        private InfosPage _page;

        private ApkInfo _apkInfo = null;
        public ApkInfo ApkInfo
        {
            get => _apkInfo;
            set
            {
                if (_apkInfo != value)
                {
                    _apkInfo = value;
                    UpdateInfos(value);
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _permissions;
        public string Permissions
        {
            get => _permissions;
            set
            {
                if (_permissions != value)
                {
                    _permissions = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public InfosViewModel(ApkInfo Info, InfosPage Page)
        {
            _page = Page;
            ApkInfo = Info ?? new();
        }

        private void UpdateInfos(ApkInfo value)
        {
            Permissions = string.Join('\n', value.Permissions);
        }
    }
}
