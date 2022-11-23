using AAPTForNet.Models;
using APKInstaller.Pages.AboutPages;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;

namespace APKInstaller.ViewModels.AboutPages
{
    public class InfosViewModel : INotifyPropertyChanged
    {
        private InfosPage _page;
        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("InfosPage");

        public string TitleFormat => _loader.GetString("TitleFormat");
        public string PermissionsHeaderFormat => _loader.GetString("PermissionsHeaderFormat");
        public string DependenciesHeaderFormat => _loader.GetString("DependenciesHeaderFormat");

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
