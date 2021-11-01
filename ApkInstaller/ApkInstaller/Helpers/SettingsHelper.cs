using AdvancedSharpAdbClient;
using CommunityToolkit.WinUI.Helpers;
using System.Threading.Tasks;

namespace APKInstaller.Helpers
{
    internal static partial class SettingsHelper
    {
        public const string IsOnlyWSA = "IsOnlyWSA";
        public const string IsFirstRun = "IsFirstRun";
        public const string DefaultDevice = "DefaultDevice";

        public static Type Get<Type>(string key) => LocalObject.Read<Type>(key);
        public static void Set(string key, object value) => LocalObject.Save(key, value);
        public static void SetFile(string key, object value) => LocalObject.CreateFileAsync(key, value);
        public static async Task<Type> GetFile<Type>(string key) => await LocalObject.ReadFileAsync<Type>(key);

        public static void SetDefaultSettings()
        {
            if (!LocalObject.KeyExists(IsOnlyWSA))
            {
                LocalObject.Save(IsOnlyWSA, SystemInformation.Instance.OperatingSystemVersion.Build >= 22000);
            }
            if (!LocalObject.KeyExists(IsFirstRun))
            {
                LocalObject.Save(IsFirstRun, true);
            }
            if (!LocalObject.KeyExists(DefaultDevice))
            {
                LocalObject.Save(DefaultDevice, new DeviceData());
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
        private static readonly ApplicationDataStorageHelper LocalObject = ApplicationDataStorageHelper.GetCurrent(new CommunityToolkit.Common.Helpers.SystemSerializer());

        static SettingsHelper()
        {
            SetDefaultSettings();
        }
    }
}
