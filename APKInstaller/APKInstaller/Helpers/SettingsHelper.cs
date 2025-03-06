using AdvancedSharpAdbClient.Models;
using APKInstaller.Models;
using CommunityToolkit.WinUI.Helpers;
using MetroLog;
using MetroLog.Targets;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.ViewManagement;
using IObjectSerializer = CommunityToolkit.Common.Helpers.IObjectSerializer;

namespace APKInstaller.Helpers
{
    public static partial class SettingsHelper
    {
        public const string ADBPath = nameof(ADBPath);
        public const string IsOpenApp = nameof(IsOpenApp);
        public const string IsOnlyWSA = nameof(IsOnlyWSA);
        public const string UpdateDate = nameof(UpdateDate);
        public const string IsFirstRun = nameof(IsFirstRun);
        public const string IsCloseADB = nameof(IsCloseADB);
        public const string IsCloseAPP = nameof(IsCloseAPP);
        public const string ShowDialogs = nameof(ShowDialogs);
        public const string ShowMessages = nameof(ShowMessages);
        public const string IsUploadAPK = nameof(IsUploadAPK);
        public const string AutoGetNetAPK = nameof(AutoGetNetAPK);
        public const string DefaultDevice = nameof(DefaultDevice);
        public const string CurrentLanguage = nameof(CurrentLanguage);
        public const string ScanPairedDevice = nameof(ScanPairedDevice);
        public const string SelectedAppTheme = nameof(SelectedAppTheme);
        public const string SelectedBackdrop = nameof(SelectedBackdrop);

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
            if (!LocalObject.KeyExists(IsUploadAPK))
            {
                LocalObject.Save(IsUploadAPK, true);
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
            if (!LocalObject.KeyExists(ScanPairedDevice))
            {
                LocalObject.Save(ScanPairedDevice, false);
            }
            if (!LocalObject.KeyExists(SelectedAppTheme))
            {
                LocalObject.Save(SelectedAppTheme, ElementTheme.Default);
            }
            if (!LocalObject.KeyExists(SelectedBackdrop))
            {
                LocalObject.Save(SelectedBackdrop, BackdropType.Mica);
            }
        }
    }

    public static partial class SettingsHelper
    {
        public static readonly UISettings UISettings = new();
        public static readonly ILogManager LogManager = LogManagerFactory.CreateLogManager(GetDefaultReleaseConfiguration());
        public static OSVersion OperatingSystemVersion => SystemInformation.Instance.OperatingSystemVersion;
        private static readonly ApplicationDataStorageHelper LocalObject = ApplicationDataStorageHelper.GetCurrent(new SystemTextJsonObjectSerializer());

        static SettingsHelper() => SetDefaultSettings();

        public static void CheckAssembly()
        {
            LogManager.GetLogger("Hello World!").Info("\nThis is a hello from the author @wherewhere.\nIf you can't find this hello in your installed version, that means you have installed a piracy one.\nRemember, the author is @wherewhere. If not, you possible install a modified one too.");
            AssemblyName Info = Assembly.GetExecutingAssembly().GetName();
            if (Info.Name != $"{"APK"}{"Installer"}") { LogManager.GetLogger("Check Assembly").Error($"\nAssembly name is wrong.\nThe wrong name is {Info.Name}.\nIt should be {$"{"APK"}{"Installer"}"}."); };
            if (Info.Version.Major != Package.Current.Id.Version.Major || Info.Version.Minor != Package.Current.Id.Version.Minor || Info.Version.Build != Package.Current.Id.Version.Build) { LogManager.GetLogger("CheckAssembly").Error($"\nAssembly version is wrong.\nThe wrong version is {Info.Version}.\nIt should be {Package.Current.Id.Version.ToFormattedString()}."); };
        }

        private static LoggingConfiguration GetDefaultReleaseConfiguration()
        {
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "MetroLogs");
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            LoggingConfiguration loggingConfiguration = new();
            loggingConfiguration.AddTarget(LogLevel.Info, LogLevel.Fatal, new StreamingFileTarget(path, 7));
            return loggingConfiguration;
        }
    }

    public class SystemTextJsonObjectSerializer : IObjectSerializer
    {
        public string Serialize<T>(T value) => value switch
        {
            bool => JsonSerializer.Serialize(value, SourceGenerationContext.Default.Boolean),
            string => JsonSerializer.Serialize(value, SourceGenerationContext.Default.String),
            DateTime => JsonSerializer.Serialize(value, SourceGenerationContext.Default.DateTime),
            DeviceData => JsonSerializer.Serialize(value, SourceGenerationContext.Default.DeviceData),
            ElementTheme => JsonSerializer.Serialize(value, SourceGenerationContext.Default.ElementTheme),
            BackdropType => JsonSerializer.Serialize(value, SourceGenerationContext.Default.BackdropType),
#if DEBUG
            _ => JsonSerializer.Serialize(value)
#else
            _ => value?.ToString(),
#endif
        };

        public T Deserialize<T>([StringSyntax(StringSyntaxAttribute.Json)] string value)
        {
            if (string.IsNullOrEmpty(value)) { return default; }
            Type type = typeof(T);
            return type == typeof(bool) ? Deserialize(value, SourceGenerationContext.Default.Boolean)
                : type == typeof(string) ? Deserialize(value, SourceGenerationContext.Default.String)
                : type == typeof(DateTime) ? Deserialize(value, SourceGenerationContext.Default.DateTime)
                : type == typeof(DeviceData) ? Deserialize(value, SourceGenerationContext.Default.DeviceData)
                : type == typeof(ElementTheme) ? Deserialize(value, SourceGenerationContext.Default.ElementTheme)
                : type == typeof(BackdropType) ? Deserialize(value, SourceGenerationContext.Default.BackdropType)
#if DEBUG
                : JsonSerializer.Deserialize<T>(value);
#else
                : default;
#endif
            static T Deserialize<TValue>([StringSyntax(StringSyntaxAttribute.Json)] string json, JsonTypeInfo<TValue> jsonTypeInfo) => JsonSerializer.Deserialize(json, jsonTypeInfo) is T value ? value : default;
        }
    }

    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(DateTime))]
    [JsonSerializable(typeof(DeviceData))]
    [JsonSerializable(typeof(UpdateInfo))]
    [JsonSerializable(typeof(ElementTheme))]
    [JsonSerializable(typeof(BackdropType))]
    public partial class SourceGenerationContext : JsonSerializerContext;
}
