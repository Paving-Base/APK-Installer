using System;
using System.Collections.Generic;

namespace Zeroconf
{
    public static partial class ZeroconfResolver
    {
        public static ResolverListener CreateListener(
            IEnumerable<string> protocols,
            int queryInterval = 4000,
            int pingsUntilRemove = 2,
            TimeSpan scanTime = default,
            int retries = 2,
            int retryDelayMilliseconds = 2000)
        {
            return new ResolverListener(protocols, queryInterval, pingsUntilRemove, scanTime, retries, retryDelayMilliseconds);
        }

        public static ResolverListener CreateListener(
            string protocol,
            int queryInterval = 4000,
            int pingsUntilRemove = 2,
            TimeSpan scanTime = default,
            int retries = 2,
            int retryDelayMilliseconds = 2000)
        {
            return CreateListener(new[] { protocol }, queryInterval, pingsUntilRemove, scanTime, retries, retryDelayMilliseconds);
        }

    }
}
