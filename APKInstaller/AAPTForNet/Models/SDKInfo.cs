using System;
using System.Collections.Generic;

namespace AAPTForNet.Models
{
    public class SDKInfo
    {
        internal static readonly SDKInfo Unknown = new("0", "0", "0");

        // https://source.android.com/setup/start/build-numbers
        private static readonly string[] AndroidCodeNames = {
            "Unknown",
            "Unnamed",  // API level 1
            "Unnamed",
            "Cupcake",
            "Donut",
            "Eclair",
            "Eclair",
            "Eclair",
            "Froyo",
            "Gingerbread",
            "Gingerbread",
            "Honeycomb",
            "Honeycomb",
            "Honeycomb",
            "Ice Cream Sandwich",
            "Ice Cream Sandwich",
            "Jelly Bean",
            "Jelly Bean",
            "Jelly Bean",
            "KitKat",
            "Unknown",  // API level 20
            "Lollipop",
            "Lollipop",
            "Marshmallow",
            "Nougat",
            "Nougat",
            "Oreo",
            "Oreo",
            "Pie",
            "Q",
            "R",        // API level 30
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z"         // API level 38
        };

        private static readonly string[] AndroidVersionCodes = {
            "Unknown",
            "1.0",  // API level 1
            "1.1",
            "1.5",
            "1.6",
            "2.0",
            "2.0",
            "2.1",
            "2.2",
            "2.3",
            "2.3",
            "3.0",
            "3.1",
            "3.2",
            "4.0",
            "4.0",
            "4.1",
            "4.2",
            "4.3",
            "4.4",
            "4.4W", // API level 20
            "5.0",
            "5.1",
            "6.0",
            "7.0",
            "7.1",
            "8.0",
            "8.1",
            "9",
            "10",
            "11",   // API level 30
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19"    // API level 38
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
            int index = (sdkVer < 1 || sdkVer > AndroidCodeNames.Length - 1) ? 0 : sdkVer;

            return new SDKInfo(sdkVer.ToString(),
                AndroidVersionCodes[index], AndroidCodeNames[index]);
        }

        public static SDKInfo GetInfo(string sdkVer)
        {
            return int.TryParse(sdkVer, out int ver)
                ? ver > AndroidVersionCodes.Length - 1 ? new SDKInfo(sdkVer, sdkVer, "Hello from 2022!") : GetInfo(ver)
                : new SDKInfo(sdkVer, sdkVer, AndroidCodeNames[0]);
        }

        public override int GetHashCode() => 1008763889 + EqualityComparer<string>.Default.GetHashCode(APILevel);

        public override bool Equals(object obj)
        {
            return obj is SDKInfo another && APILevel == another.APILevel;
        }

        public int CompareTo(object obj)
        {
            return obj is SDKInfo another
                ? int.TryParse(APILevel, out int ver) && int.TryParse(another.APILevel, out int anotherver) ? ver.CompareTo(anotherver) : 0
                : throw new ArgumentException();
        }

        public static bool operator ==(SDKInfo left, SDKInfo right) => left.Equals(right);

        public static bool operator !=(SDKInfo left, SDKInfo right) => !(left == right);

        public static bool operator <(SDKInfo left, SDKInfo right) => left.CompareTo(right) < 0;

        public static bool operator <=(SDKInfo left, SDKInfo right) => left.CompareTo(right) <= 0;

        public static bool operator >(SDKInfo left, SDKInfo right) => left.CompareTo(right) > 0;

        public static bool operator >=(SDKInfo left, SDKInfo right) => left.CompareTo(right) >= 0;

        public override string ToString()
        {
            return this == Unknown
                ? AndroidCodeNames[0]
                : $"API Level {APILevel} " +
                $"{(Version == AndroidVersionCodes[0] ? $"({Version} - " : $"(Android {Version} - ")}" +
                $"{CodeName})";
        }
    }
}
