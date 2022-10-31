using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace APKInstaller.Helpers
{
    public static class OSVersionHelper
    {
        public static readonly Version OSVersion = GetOSVersion();

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

        private static Version GetOSVersion()
        {
            RTL_OSVERSIONINFOEX osv = new();
            osv.dwOSVersionInfoSize = (uint)Marshal.SizeOf(osv);
            int ret = RtlGetVersion(out osv);
            Debug.Assert(ret == 0);
            return new Version((int)osv.dwMajorVersion, (int)osv.dwMinorVersion, (int)osv.dwBuildNumber);
        }

        [DllImport("ntdll.dll")]
        private static extern int RtlGetVersion(out RTL_OSVERSIONINFOEX lpVersionInformation);

        [StructLayout(LayoutKind.Sequential)]
        private struct RTL_OSVERSIONINFOEX
        {
            internal uint dwOSVersionInfoSize;
            internal uint dwMajorVersion;
            internal uint dwMinorVersion;
            internal uint dwBuildNumber;
            internal uint dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            internal string szCSDVersion;
        }
    }
}
