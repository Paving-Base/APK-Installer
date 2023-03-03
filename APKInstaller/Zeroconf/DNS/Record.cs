// Stuff records are made of

namespace Zeroconf.DNS
{
    internal abstract class Record
    {
        /// <summary>
        /// The Resource Record this RDATA record belongs to
        /// </summary>
        public RR RR;
    }
}
