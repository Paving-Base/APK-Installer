using System;
using System.Runtime.Versioning;

namespace APKInstaller.Helpers
{
    public static class OSVersionHelper
    {
        public static readonly Version OSVersion = Environment.OSVersion.Version;

        /// <summary>
        /// Whether the operating system version is greater than or equal to 6.1.
        /// </summary>
        [SupportedOSPlatformGuard("windows7")]
        public static bool IsWindows7OrGreater { get; } = OperatingSystem.IsWindowsVersionAtLeast(major: 6, minor: 1);

        /// <summary>
        /// Whether the operating system version build is greater than the value.
        /// </summary>
        /// <param name="Build">Build verson</param>
        public static bool IsOSVersonGreater(this int Build) => OSVersion.Build >= Build;
    }
}
