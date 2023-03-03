using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Zeroconf.Models;

namespace Zeroconf.Interfaces
{
    internal interface INetworkInterface
    {
        Task NetworkRequestAsync(
            byte[] requestBytes,
            TimeSpan scanTime,
            int retries,
            int retryDelayMilliseconds,
            Action<IPAddress, byte[]> onResponse,
            CancellationToken cancellationToken,
            IEnumerable<System.Net.NetworkInformation.NetworkInterface> netInterfacesToSendRequestOn);

        Task ListenForAnnouncementsAsync(Action<AdapterInformation, string, byte[]> callback, CancellationToken cancellationToken);
    }
}
