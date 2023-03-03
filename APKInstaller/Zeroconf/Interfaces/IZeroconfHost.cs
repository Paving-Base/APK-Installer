using System.Collections.Generic;

namespace Zeroconf.Interfaces
{
    /// <summary>
    /// A ZeroConf record response
    /// </summary>
    public interface IZeroconfHost
    {
        /// <summary>
        /// Name
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Id, possibly different than the Name
        /// </summary>
        string Id { get; }

        /// <summary>
        /// IP Address (alias for IPAddresses.First())
        /// </summary>
        string IPAddress { get; }

        /// <summary>
        /// IP Addresses
        /// </summary>
        IReadOnlyList<string> IPAddresses { get; }


        /// <summary>
        /// Services offered by this host (based on services queried for)
        /// </summary>
        IReadOnlyDictionary<string, IService> Services { get; }
    }
}
