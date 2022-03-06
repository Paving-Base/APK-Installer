using System.Collections.Generic;

namespace AAPTForNet.Models
{
    public class SDKInfo
    {
        internal static readonly SDKInfo Unknown = new SDKInfo("0", "0", "0");

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
            "R",  // API level 30
            "S",
            "T"
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
            "4.4W",  // API level 20
            "5.0",
            "5.1",
            "6.0",
            "7.0",
            "7.1",
            "8.0",
            "8.1",
            "9",
            "10",
            "11",    // API level 30
            "12",
            "13"
        };

        public string APILever { get; }
        public string Version { get; }
        public string CodeName { get; }

        protected SDKInfo(string level, string ver, string code)
        {
            APILever = level;
            Version = ver;
            CodeName = code;
        }

        public static SDKInfo GetInfo(int sdkVer)
        {
            int index = sdkVer < 1 || sdkVer > AndroidCodeNames.Length - 1 ? 0 : sdkVer;

            return new SDKInfo(sdkVer.ToString(),
                AndroidVersionCodes[index], AndroidCodeNames[index]);
        }

        public static SDKInfo GetInfo(string sdkVer)
        {
            int.TryParse(sdkVer, out int ver);
            return GetInfo(ver);
        }

        public override int GetHashCode() => 1008763889 + EqualityComparer<string>.Default.GetHashCode(APILever);

        public override bool Equals(object obj)
        {
            if (obj is SDKInfo another)
            {
                return APILever == another.APILever;
            }
            return false;
        }

        public override string ToString()
        {
            if (APILever.Equals("0") && Version.Equals("0") && CodeName.Equals("0"))
            {
                return AndroidCodeNames[0];
            }

            return $"API Level {APILever} " +
                $"{(Version == AndroidCodeNames[0] ? $"({AndroidCodeNames[0]} - " : $"(Android {Version} - ")}" +
                $"{CodeName})";
        }
    }
}
