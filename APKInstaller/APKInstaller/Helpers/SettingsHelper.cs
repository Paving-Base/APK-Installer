using CommunityToolkit.WinUI.Helpers;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace APKInstaller.Helpers
{
    internal static partial class SettingsHelper
    {
        public const string IsFirstRun = "IsFirstRun";

        public static Type Get<Type>(string key) => LocalObject.Read<Type>(key);
        public static void Set(string key, object value) => LocalObject.Save(key, value);
        public static void SetFile(string key, object value) => LocalObject.SaveFileAsync(key, value);

        public static void SetDefaultSettings()
        {
            if (!LocalObject.KeyExists(IsFirstRun))
            {
                LocalObject.Save(IsFirstRun, true);
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
        private static readonly LocalObjectStorageHelper LocalObject = new LocalObjectStorageHelper(null);

        static SettingsHelper()
        {
            SetDefaultSettings();
        }
    }
}
