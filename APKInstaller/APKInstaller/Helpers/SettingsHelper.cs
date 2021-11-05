using AdvancedSharpAdbClient;
using CommunityToolkit.WinUI.Helpers;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace APKInstaller.Helpers
{
    internal static partial class SettingsHelper
    {
        public const string IsOpenApp = "IsOpenApp";
        public const string IsOnlyWSA = "IsOnlyWSA";
        public const string IsFirstRun = "IsFirstRun";
        public const string DefaultDevice = "DefaultDevice";

        public static Type Get<Type>(string key) => LocalObject.Read<Type>(key);
        public static void Set(string key, object value) => LocalObject.Save(key, value);
        public static void SetFile(string key, object value) => LocalObject.SaveFileAsync(key, value);
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
        private static readonly LocalObjectStorageHelper LocalObject = new LocalObjectStorageHelper(new SystemTextJsonSerializer());
        public static OSVersion OperatingSystemVersion => SystemInformation.Instance.OperatingSystemVersion;

        static SettingsHelper()
        {
            SetDefaultSettings();
        }
    }

    internal class SystemTextJsonSerializer : IObjectSerializer
    {
        public object Serialize<T>(T value) => JsonSerializer.Serialize(value);

        public T Deserialize<T>(object value) => JsonSerializer.Deserialize<T>((string)value);
    }
}
