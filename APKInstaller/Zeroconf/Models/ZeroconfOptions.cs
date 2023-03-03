using System;
using System.Collections.Generic;

namespace Zeroconf.Models
{
    public enum ScanQueryType
    {
        Ptr,
        Any
    }

    public abstract class ZeroconfOptions
    {
        private int retries;

        public TimeSpan ScanTime { get; set; }
        public TimeSpan RetryDelay { get; set; }

        public IEnumerable<string> Protocols { get; }

        public bool AllowOverlappedQueries { get; set; }

        public ScanQueryType ScanQueryType { get; set; }

        public int Retries
        {
            get => retries;
            set => retries = value < 0 ? throw new ArgumentOutOfRangeException(nameof(value)) : value;
        }

        protected ZeroconfOptions(string protocol) : this(new[] { protocol })
        {
        }

        protected ZeroconfOptions(IEnumerable<string> protocols)
        {
            Protocols = new HashSet<string>(protocols
                ?? throw new ArgumentNullException(nameof(protocols)), StringComparer.OrdinalIgnoreCase);
            Retries = 2;
            RetryDelay = TimeSpan.FromSeconds(2);
            ScanTime = TimeSpan.FromSeconds(2);
            ScanQueryType = ScanQueryType.Ptr;
        }

    }

    public class BrowseDomainsOptions : ZeroconfOptions
    {
        public BrowseDomainsOptions() : base("_services._dns-sd._udp.local.")
        {
        }
    }

    public class ResolveOptions : ZeroconfOptions
    {
        public ResolveOptions(string protocol) : base(protocol)
        {
        }

        public ResolveOptions(IEnumerable<string> protocols) : base(protocols)
        {
        }
    }
}