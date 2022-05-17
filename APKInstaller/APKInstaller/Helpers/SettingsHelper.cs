using AdvancedSharpAdbClient;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.ViewManagement;

namespace APKInstaller.Helpers
{
    internal static partial class SettingsHelper
    {
        public const string ADBPath = "ADBPath";
        public const string IsOpenApp = "IsOpenApp";
        public const string IsOnlyWSA = "IsOnlyWSA";
        public const string UpdateDate = "UpdateDate";
        public const string IsFirstRun = "IsFirstRun";
        public const string IsCloseADB = "IsCloseADB";
        public const string IsCloseAPP = "IsCloseAPP";
        public const string ShowDialogs = "ShowDialogs";
        public const string ShowMessages = "ShowMessages";
        public const string AutoGetNetAPK = "AutoGetNetAPK";
        public const string DefaultDevice = "DefaultDevice";
        public const string CurrentLanguage = "CurrentLanguage";
        public const string SelectedAppTheme = "SelectedAppTheme";

        public static Type Get<Type>(string key) => LocalObject.Read<Type>(key);
        public static void Set(string key, object value) => LocalObject.Save(key, value);
        public static void SetFile(string key, object value) => LocalObject.CreateFileAsync(key, value);
        public static async Task<Type> GetFile<Type>(string key) => await LocalObject.ReadFileAsync<Type>(key);

        public static void SetDefaultSettings()
        {
            if (!LocalObject.KeyExists(ADBPath))
            {
                LocalObject.Save(ADBPath, Path.Combine(ApplicationData.Current.LocalFolder.Path, @"platform-tools\adb.exe"));
            }
            if (!LocalObject.KeyExists(IsOpenApp))
            {
                LocalObject.Save(IsOpenApp, true);
            }
            if (!LocalObject.KeyExists(IsOnlyWSA))
            {
                LocalObject.Save(IsOnlyWSA, OperatingSystemVersion.Build >= 22000);
            }
            if (!LocalObject.KeyExists(UpdateDate))
            {
                LocalObject.Save(UpdateDate, new DateTime());
            }
            if (!LocalObject.KeyExists(IsFirstRun))
            {
                LocalObject.Save(IsFirstRun, true);
            }
            if (!LocalObject.KeyExists(IsCloseADB))
            {
                LocalObject.Save(IsCloseADB, true);
            }
            if (!LocalObject.KeyExists(IsCloseAPP))
            {
                LocalObject.Save(IsCloseAPP, true);
            }
            if (!LocalObject.KeyExists(ShowDialogs))
            {
                LocalObject.Save(ShowDialogs, true);
            }
            if (!LocalObject.KeyExists(ShowMessages))
            {
                LocalObject.Save(ShowMessages, true);
            }
            if (!LocalObject.KeyExists(AutoGetNetAPK))
            {
                LocalObject.Save(AutoGetNetAPK, false);
            }
            if (!LocalObject.KeyExists(DefaultDevice))
            {
                LocalObject.Save(DefaultDevice, new DeviceData());
            }
            if (!LocalObject.KeyExists(CurrentLanguage))
            {
                LocalObject.Save(CurrentLanguage, LanguageHelper.AutoLanguageCode);
            }
            if (!LocalObject.KeyExists(SelectedAppTheme))
            {
                LocalObject.Save(SelectedAppTheme, ElementTheme.Default);
            }
        }
    }

    public enum UISettingChangedType
    {
        LightMode,
        DarkMode,
        NoPicChanged,
    }

    internal static partial class SettingsHelper
    {
        public static readonly UISettings UISettings = new();
        public static OSVersion OperatingSystemVersion => SystemInformation.Instance.OperatingSystemVersion;
        private static readonly ApplicationDataStorageHelper LocalObject = ApplicationDataStorageHelper.GetCurrent(new SystemTextJsonObjectSerializer());
        public static ElementTheme Theme => Get<bool>("IsBackgroundColorFollowSystem") ? ElementTheme.Default : (Get<bool>("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light);

        static SettingsHelper()
        {
            SetDefaultSettings();
        }
    }

    public class SystemTextJsonObjectSerializer : CommunityToolkit.Common.Helpers.IObjectSerializer
    {
        string CommunityToolkit.Common.Helpers.IObjectSerializer.Serialize<T>(T value) => JsonSerializer.Serialize(value);

        public T Deserialize<T>(string value) => JsonSerializer.Deserialize<T>(value);
    }
}
