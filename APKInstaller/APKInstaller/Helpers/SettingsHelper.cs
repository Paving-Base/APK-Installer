using AdvancedSharpAdbClient;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace APKInstaller.Helpers
{
    internal static partial class SettingsHelper
    {
        public const string IsOpenApp = "IsOpenApp";
        public const string IsOnlyWSA = "IsOnlyWSA";
        public const string IsDarkMode = "IsDarkMode";
        public const string UpdateDate = "UpdateDate";
        public const string IsFirstRun = "IsFirstRun";
        public const string IsCloseADB = "IsCloseADB";
        public const string DefaultDevice = "DefaultDevice";
        public const string IsBackgroundColorFollowSystem = "IsBackgroundColorFollowSystem";

        public static Type Get<Type>(string key) => LocalObject.Read<Type>(key);
        public static void Set(string key, object value) => LocalObject.Save(key, value);
        public static void SetFile(string key, object value) => LocalObject.CreateFileAsync(key, value);
        public static async Task<Type> GetFile<Type>(string key) => await LocalObject.ReadFileAsync<Type>(key);

        public static void SetDefaultSettings()
        {
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
            if (!LocalObject.KeyExists(IsDarkMode))
            {
                LocalObject.Save(IsDarkMode, false);
            }
            if (!LocalObject.KeyExists(IsCloseADB))
            {
                LocalObject.Save(IsCloseADB, false);
            }
            if (!LocalObject.KeyExists(DefaultDevice))
            {
                LocalObject.Save(DefaultDevice, new DeviceData());
            }
            if (!LocalObject.KeyExists(IsBackgroundColorFollowSystem))
            {
                LocalObject.Save(IsBackgroundColorFollowSystem, true);
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
