using System.Collections.Generic;

namespace Zeroconf.Interfaces
{
    /// <summary>
    /// Represents a service provided by a host
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// This is the name retrieved from the PTR record e.g. _http._tcp.local.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// This is the name retrieved from the SRV record e.g. myserver._http._tcp.local.
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// Port
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Time-to-live
        /// </summary>
        int Ttl { get; }

        /// <summary>
        /// Properties of the object. Most services have a single set of properties, but some service may return multiple sets of properties
        /// </summary>
        IReadOnlyList<IReadOnlyDictionary<string, string>> Properties { get; }
    }

}
