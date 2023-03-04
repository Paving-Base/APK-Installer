using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeroconf.Interfaces;

namespace Zeroconf.Models
{
    /// <summary>
    /// A ZeroConf record response
    /// </summary>
    internal class ZeroconfHost : IZeroconfHost, IEquatable<ZeroconfHost>, IEquatable<IZeroconfHost>
    {
        private readonly Dictionary<string, IService> services = new();

        /// <summary>
        /// Id, possibly different than the display name
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display Name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// IP Address (alias for IPAddresses.First())
        /// </summary>
        public string IPAddress => IPAddresses.FirstOrDefault();

        /// <summary>
        /// IP Addresses
        /// </summary>
        public IReadOnlyList<string> IPAddresses { get; set; }

        /// <summary>
        /// Collection of services provided by the host
        /// </summary>
        public IReadOnlyDictionary<string, IService> Services => services;

        internal void AddService(IService service)
        {
            services[service.ServiceName] = service ?? throw new ArgumentNullException(nameof(service));
        }

        public bool Equals(IZeroconfHost other)
        {
            return Equals(other as ZeroconfHost);
        }

        public bool Equals(ZeroconfHost other)
        {
            return other is not null && (ReferenceEquals(this, other) || (string.Equals(Id, other.Id) && string.Equals(IPAddress, other.IPAddress)));
        }

        public override bool Equals(object obj)
        {
            return obj is not null && (ReferenceEquals(this, obj) || (obj.GetType() == GetType() && Equals((ZeroconfHost)obj)));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int addressesHash = IPAddresses?.Aggregate(0, (current, address) => (current * 397) ^ address.GetHashCode()) ?? 0;
                return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ addressesHash;
            }
        }

        /// <summary>
        /// Diagnostic
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine("| ----------------------------------------------");
            sb.AppendLine("| HOST");
            sb.AppendLine("| ----------------------------------------------");
            sb.AppendLine($"| Id: {Id}\n| DisplayName: {DisplayName}\n| IPs: {string.Join(", ", IPAddresses)}\n| Services: {services.Count}");

            if (services.Any())
            {
                int i = 0;
                foreach (KeyValuePair<string, IService> service in services)
                {
                    sb.AppendLine("\t| -------------------");
                    sb.AppendLine($"\t| Service #{i++}");
                    sb.AppendLine("\t| -------------------");
                    sb.AppendLine(service.Value.ToString());
                    sb.AppendLine("\t| -------------------");
                }

            }

            sb.AppendLine("| ---------------------------------------------");

            return sb.ToString();
        }
    }
}
