using AdvancedSharpAdbClient;
using APKInstaller.Helpers;
using APKInstaller.Models;
using APKInstaller.Pages.SettingsPages;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;

namespace APKInstaller.ViewModels.SettingsPages
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly SettingsPage _page;
        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("SettingsPage");

        private IEnumerable<DeviceData> _deviceList;
        public IEnumerable<DeviceData> DeviceList
        {
            get => _deviceList;
            set
            {
                _deviceList = value;
                RaisePropertyChangedEvent();
                if (!IsOnlyWSA) { ChooseDevice(); }
            }
        }

        private bool _isOnlyWSA = SettingsHelper.Get<bool>(SettingsHelper.IsOnlyWSA);
        public bool IsOnlyWSA
        {
            get => _isOnlyWSA;
            set
            {
                SettingsHelper.Set(SettingsHelper.IsOnlyWSA, value);
                _isOnlyWSA = SettingsHelper.Get<bool>(SettingsHelper.IsOnlyWSA);
                _page.SelectDeviceBox.SelectionMode = value ? ListViewSelectionMode.None : ListViewSelectionMode.Single;
                if (!value) { ChooseDevice(); }
                RaisePropertyChangedEvent();
            }
        }

        private bool _isCloseADB = SettingsHelper.Get<bool>(SettingsHelper.IsCloseADB);
        public bool IsCloseADB
        {
            get => _isCloseADB;
            set
            {
                SettingsHelper.Set(SettingsHelper.IsCloseADB, value);
                _isCloseADB = SettingsHelper.Get<bool>(SettingsHelper.IsCloseADB);
            }
        }

        private DateTime _updateDate = SettingsHelper.Get<DateTime>(SettingsHelper.UpdateDate);
        public DateTime UpdateDate
        {
            get => _updateDate;
            set
            {
                SettingsHelper.Set(SettingsHelper.UpdateDate, value);
                _updateDate = SettingsHelper.Get<DateTime>(SettingsHelper.UpdateDate);
                RaisePropertyChangedEvent();
            }
        }

        private bool _autoGetNetAPK = SettingsHelper.Get<bool>(SettingsHelper.AutoGetNetAPK);
        public bool AutoGetNetAPK
        {
            get => _autoGetNetAPK;
            set
            {
                SettingsHelper.Set(SettingsHelper.AutoGetNetAPK, value);
                _autoGetNetAPK = SettingsHelper.Get<bool>(SettingsHelper.AutoGetNetAPK);
            }
        }

        private bool _checkingUpdate;
        public bool CheckingUpdate
        {
            get => _checkingUpdate;
            set
            {
                _checkingUpdate = value;
                RaisePropertyChangedEvent();
            }
        }

        private string _gotoUpdateTag;
        public string GotoUpdateTag
        {
            get => _gotoUpdateTag;
            set
            {
                _gotoUpdateTag = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _gotoUpdateVisibility;
        public Visibility GotoUpdateVisibility
        {
            get => _gotoUpdateVisibility;
            set
            {
                _gotoUpdateVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _updateStateIsOpen;
        public bool UpdateStateIsOpen
        {
            get => _updateStateIsOpen;
            set
            {
                _updateStateIsOpen = value;
                RaisePropertyChangedEvent();
            }
        }

        private string _updateStateMessage;
        public string UpdateStateMessage
        {
            get => _updateStateMessage;
            set
            {
                _updateStateMessage = value;
                RaisePropertyChangedEvent();
            }
        }

        private InfoBarSeverity _updateStateSeverity;
        public InfoBarSeverity UpdateStateSeverity
        {
            get => _updateStateSeverity;
            set
            {
                _updateStateSeverity = value;
                RaisePropertyChangedEvent();
            }
        }

        private string _updateStateTitle;
        public string UpdateStateTitle
        {
            get => _updateStateTitle;
            set
            {
                _updateStateTitle = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public static string VersionTextBlockText
        {
            get
            {
                string ver = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();
                string name = loader?.GetString("AppName") ?? "APK Installer";
                return $"{name} v{ver}";
            }
        }

        public SettingsViewModel(SettingsPage Page) => _page = Page;

        public void OnDeviceChanged(object sender, DeviceDataEventArgs e)
        {
            UIHelper.DispatcherQueue?.EnqueueAsync(() =>
            {
                DeviceList = new AdvancedAdbClient().GetDevices();
            });
        }

        public async void CheckUpdate()
        {
            CheckingUpdate = true;
            UpdateInfo info = null;
            try
            {
                info = await UpdateHelper.CheckUpdateAsync("Paving-Base", "APK-Installer");
            }
            catch (Exception ex)
            {
                UpdateStateIsOpen = true;
                UpdateStateMessage = ex.Message;
                UpdateStateSeverity = InfoBarSeverity.Error;
                GotoUpdateVisibility = Visibility.Collapsed;
                UpdateStateTitle = _loader.GetString("CheckFailed");
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
    }
}
