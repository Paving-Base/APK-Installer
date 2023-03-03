using System;

namespace Zeroconf.Models
{
    public readonly struct DomainService : IEquatable<DomainService>
    {
        public string Domain { get; }
        public string Service { get; }

        public DomainService(string domain, string service)
        {
            Domain = domain;
            Service = service;
        }

        public void Deconstruct(out string domain, out string service)
        {
            domain = Domain;
            service = Service;
        }

        public bool Equals(DomainService other)
        {
            return string.Equals(Domain, other.Domain) && string.Equals(Service, other.Service);
        }

        public override bool Equals(object obj)
        {
            return obj is not null && obj is DomainService && Equals((DomainService)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Domain != null ? Domain.GetHashCode() : 0) * 397) ^ (Service != null ? Service.GetHashCode() : 0);
            }
        }

        public static bool operator ==(DomainService left, DomainService right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DomainService left, DomainService right)
        {
            return !left.Equals(right);
        }
    }
}
