using System;

namespace APKInstaller.Models
{
    public readonly struct SystemVersionInfo
    {
        public SystemVersionInfo(int major, int minor, int build, int revision = 0)
        {
            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
        }

        public int Major { get; }

        public int Minor { get; }

        public int Build { get; }

        public int Revision { get; }

        public bool Equals(SystemVersionInfo other) => Major == other.Major && Minor == other.Minor && Build == other.Build && Revision == other.Revision;

        public override bool Equals(object obj) => obj is SystemVersionInfo other && Equals(other);

        public override int GetHashCode() => Major.GetHashCode() ^ Minor.GetHashCode() ^ Build.GetHashCode() ^ Revision.GetHashCode();

        public static bool operator ==(SystemVersionInfo left, SystemVersionInfo right) => left.Equals(right);

        public static bool operator !=(SystemVersionInfo left, SystemVersionInfo right) => !(left == right);

        public int CompareTo(SystemVersionInfo other)
        {
            if (Major != other.Major)
            {
                return Major.CompareTo(other.Major);
            }

            if (Minor != other.Minor)
            {
                return Minor.CompareTo(other.Minor);
            }

            if (Build != other.Build)
            {
                return Build.CompareTo(other.Build);
            }

            if (Revision != other.Revision)
            {
                return Revision.CompareTo(other.Revision);
            }

            return 0;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is SystemVersionInfo other))
            {
                throw new ArgumentException();
            }

            return CompareTo(other);
        }

        public static bool operator <(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) < 0;

        public static bool operator <=(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) <= 0;

        public static bool operator >(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) > 0;

        public static bool operator >=(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) >= 0;

        public override string ToString() => $"{Major}.{Minor}.{Build}.{Revision}";
    }
}
