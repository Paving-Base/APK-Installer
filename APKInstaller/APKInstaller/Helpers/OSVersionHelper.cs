using System;

namespace APKInstaller.Helpers
{
    public static class OSVersionHelper
    {
        public static readonly Version OSVersion = Environment.OSVersion.Version;

        /// <summary>
        /// Whether the operating system is NT. 
        /// </summary>
        public static bool IsWindowsNT { get; } = Environment.OSVersion.Platform == PlatformID.Win32NT;

        /// <summary>
        /// Whether the operating system version is greater than or equal to 6.0.
        /// </summary>
        public static bool IsWindowsVistaOrGreater { get; } = IsWindowsNT && OSVersion >= new Version(6, 0);

        /// <summary>
        /// Whether the operating system version is greater than or equal to 6.1.
        /// </summary>
        public static bool IsWindows7OrGreater { get; } = IsWindowsNT && OSVersion >= new Version(6, 1);

        /// <summary>
        /// Whether the operating system version is greater than or equal to 6.2.
        /// </summary>
        public static bool IsWindows8OrGreater { get; } = IsWindowsNT && OSVersion >= new Version(6, 2);

        /// <summary>
        /// Whether the operating system version is greater than or equal to 10.0.
        /// </summary>
        public static bool IsWindows10OrGreater { get; } = IsWindowsNT && OSVersion >= new Version(10, 0);

        /// <summary>
        /// Whether the operating system version is greater than or equal to 10.0* (build 21996).
        /// </summary>
        public static bool IsWindows11OrGreater { get; } = IsWindowsNT && OSVersion >= new Version(10, 0, 21996);

        /// <summary>
        /// Whether the operating system version build is greater than the value.
        /// </summary>
        /// <param name="Build">Build verson</param>
        public static bool IsOSVersonGreater(this int Build) => OSVersion.Build >= Build;
    }
}
