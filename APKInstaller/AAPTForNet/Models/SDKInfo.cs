using System;
using System.Collections.Generic;

namespace AAPTForNet.Models
{
    public class SDKInfo : IComparable
    {
        internal static readonly SDKInfo Unknown = new("0", "0", "0");

        // Don't trust Wiki Pedia, trust AOSP source code
        // https://developer.android.com/reference/android/os/Build.VERSION_CODES
        // IU => Increament Update
        // MR => Maintenance Release
        private static readonly string[] AndroidCodeNames = {
            "Unknown",                  // Under Alpha and Beta development or just known
            "Base",                     // API 1
            "Base Update 1",            // API 2
            "Cupcake",                  // API 3
            "Donut",                    // API 4
            "Éclair",                   // API 5
            "Éclair IU1",               // API 6
            "Éclair MR1",               // API 7
            "Froyo",                    // API 8
            "Gingerbread",              // API 9
            "Gingerbread MR1",          // API 10
            "Honeycomb",                // API 11
            "Honeycomb MR1",            // API 12
            "Honeycomb MR2",            // API 13
            "Ice Cream Sandwich",       // API 14
            "Ice Cream Sandwich MR1",   // API 15
            "Jelly Bean",               // API 16
            "Jelly Bean MR1",           // API 17
            "Jelly Bean MR2",           // API 18
            "KitKat",                   // API 19
            "KitKat Watch",             // API 20
            "Lollipop",                 // API 21
            "Lollipop MR1",             // API 22
            "Marshmallow",              // API 23
            "Nougat",                   // API 24
            "Nougat MR1",               // API 25
            "Oreo",                     // API 26
            "Oreo MR1",                 // API 27
            "Pie",                      // API 28
            "Q",                        // API 29
            "R",                        // API 30
            "S",                        // API 31
            "S V2",                     // API 32
            "Tiramisu",                 // API 33
            "Upside Down Cake",         // API 34
            "?!"                        // Who knows? But it is not yet released in April 2023!
        };

        private static readonly string[] AndroidVersionCodes = {
            "Unknown",  // Under Alpha and Beta development or just known
            "1.0",      // API 1
            "1.1",      // API 2
            "1.5",      // API 3
            "1.6",      // API 4
            "2.0",      // API 5
            "2.0.1",    // API 6
            "2.1",      // API 7
            "2.2",      // API 8
            "2.3",      // API 9
            "2.3.3",    // API 10
            "3.0",      // API 11
            "3.1",      // API 12
            "3.2",      // API 13
            "4.0",      // API 14
            "4.0.3",    // API 15
            "4.1",      // API 16
            "4.2",      // API 17
            "4.3",      // API 18
            "4.4",      // API 19
            "4.4W",     // API 20
            "5.0",      // API 21
            "5.1",      // API 22
            "6.0",      // API 23
            "7.0",      // API 24
            "7.1",      // API 25
            "8.0",      // API 26
            "8.1",      // API 27
            "9.0",      // API 28
            "10",       // API 29
            "11",       // API 30
            "12",       // API 31
            "12.1",     // API 32
            "13",       // API 33
            "14",       // API 34
            "?!",       // Who knows? But it is not yet released in April 2023!
        };

        public string APILevel { get; }
        public string Version { get; }
        public string CodeName { get; }

        protected SDKInfo(string level, string ver, string code)
        {
            APILevel = level;
            Version = ver;
            CodeName = code;
        }

        public static SDKInfo GetInfo(int sdkVer)
        {
            int index = Math.Min(Math.Max(sdkVer, 0), AndroidCodeNames.Length - 1);
            string version = sdkVer <= AndroidVersionCodes.Length - 1 ? AndroidVersionCodes[index] : (sdkVer - 20).ToString();
            return new SDKInfo(sdkVer.ToString(), version, AndroidCodeNames[index]);
        }

        public static SDKInfo GetInfo(string sdkVer) => int.TryParse(sdkVer, out int ver) ? GetInfo(ver) : new SDKInfo(sdkVer, sdkVer, AndroidCodeNames[0]);

        public override int GetHashCode() => 1008763889 + EqualityComparer<string>.Default.GetHashCode(APILevel);

        public override bool Equals(object obj) => obj is SDKInfo another && APILevel == another.APILevel;

        public int CompareTo(object obj) => obj is SDKInfo another
            ? int.TryParse(APILevel, out int ver) && int.TryParse(another.APILevel, out int anotherver)
            ? ver.CompareTo(anotherver) : 0
            : throw new ArgumentException(null, nameof(obj));

        public static bool operator ==(SDKInfo left, SDKInfo right) => left.Equals(right);

        public static bool operator !=(SDKInfo left, SDKInfo right) => !(left == right);

        public static bool operator <(SDKInfo left, SDKInfo right) => left.CompareTo(right) < 0;

        public static bool operator <=(SDKInfo left, SDKInfo right) => left.CompareTo(right) <= 0;

        public static bool operator >(SDKInfo left, SDKInfo right) => left.CompareTo(right) > 0;

        public static bool operator >=(SDKInfo left, SDKInfo right) => left.CompareTo(right) >= 0;

        public override string ToString() => this == Unknown
            ? AndroidCodeNames[0]
            : $"API Level {APILevel} " +
                $"{(Version == AndroidVersionCodes[0]
                ? $"({Version} - " : $"(Android {Version} - ")}" +
                $"{CodeName})";
    }
}
