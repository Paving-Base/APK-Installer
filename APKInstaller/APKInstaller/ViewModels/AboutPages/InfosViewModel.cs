using AAPTForNet.Models;
using APKInstaller.Helpers;
using APKInstaller.Pages.AboutPages;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;

namespace APKInstaller.ViewModels.AboutPages
{
    public class InfosViewModel : INotifyPropertyChanged
    {
        private readonly InfosPage _page;
        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("InfosPage");

        public string TitleFormat => _loader.GetString("TitleFormat");
        public string FeaturesHeaderFormat => _loader.GetString("FeaturesHeaderFormat");
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

        private string _features;
        public string Features
        {
            get => _features;
            set
            {
                if (_features != value)
                {
                    _features = value;
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

        private string _appLocaleName;
        public string AppLocaleName
        {
            get => _appLocaleName;
            set
            {
                if (_appLocaleName != value)
                {
                    _appLocaleName = value;
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
            Features = string.Join('\n', value.Features);
            Permissions = string.Join('\n', value.Permissions);
            AppLocaleName = value.GetLocaleLabel();
        }
    }
}
