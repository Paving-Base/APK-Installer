using System;

namespace Zeroconf.Models
{
    public readonly struct AdapterInformation : IEquatable<AdapterInformation>
    {
        public string Address { get; }
        public string Name { get; }

        public AdapterInformation(string address, string name)
        {
            Address = address;
            Name = name;
        }

        public void Deconstruct(out string address, out string name)
        {
            address = Address;
            name = Name;
        }

        public bool Equals(AdapterInformation other)
        {
            return string.Equals(Address, other.Address) && string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            return obj is not null && obj is AdapterInformation information && Equals(information);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Address != null ? Address.GetHashCode() : 0) * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        public static bool operator ==(AdapterInformation left, AdapterInformation right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AdapterInformation left, AdapterInformation right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{Name}: {Address}";
        }
    }
}