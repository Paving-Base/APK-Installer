using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Zeroconf.Interfaces;
using Zeroconf.Models;

namespace Zeroconf.Common
{
    internal class NetworkInterface : INetworkInterface
    {
        /// <summary>
        /// The logger to use when logging messages.
        /// </summary>
        private readonly ILogger<NetworkInterface> logger;

        public NetworkInterface(ILogger<NetworkInterface> logger = null)
        {
            this.logger = logger ?? NullLogger<NetworkInterface>.Instance;
        }

        public async Task NetworkRequestAsync(
            byte[] requestBytes,
            TimeSpan scanTime,
            int retries,
            int retryDelayMilliseconds,
            Action<IPAddress, byte[]> onResponse,
            CancellationToken cancellationToken,
            IEnumerable<System.Net.NetworkInformation.NetworkInterface> netInterfacesToSendRequestOn = null)
        {
            // populate list with all adapters if none specified
            if (netInterfacesToSendRequestOn == null || !netInterfacesToSendRequestOn.Any())
            {
                netInterfacesToSendRequestOn = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            }

            List<Task> tasks = netInterfacesToSendRequestOn
                .Select(inter =>
                    NetworkRequestAsync(requestBytes, scanTime, retries, retryDelayMilliseconds, onResponse, inter, cancellationToken))
                .ToList();

            await Task.WhenAll(tasks)
                      .ConfigureAwait(false);
        }

        private async Task NetworkRequestAsync(
            byte[] requestBytes,
            TimeSpan scanTime,
            int retries,
            int retryDelayMilliseconds,
            Action<IPAddress, byte[]> onResponse,
            System.Net.NetworkInformation.NetworkInterface adapter,
            CancellationToken cancellationToken)
        {
            // http://stackoverflow.com/questions/2192548/specifying-what-network-interface-an-udp-multicast-should-go-to-in-net

            // Xamarin doesn't support this
            if (!adapter.GetIPProperties().MulticastAddresses.Any())
            {
                return; // most of VPN adapters will be skipped
            }

            if (!adapter.SupportsMulticast)
            {
                return; // multicast is meaningless for this type of connection
            }

            if (OperationalStatus.Up != adapter.OperationalStatus)
            {
                return; // this adapter is off or not connected
            }

            if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback)
            {
                return; // strip out loopback addresses
            }

            IPv4InterfaceProperties p = adapter.GetIPProperties().GetIPv4Properties();
            if (null == p)
            {
                return; // IPv4 is not configured on this adapter
            }

            IPAddress ipv4Address = adapter.GetIPProperties().UnicastAddresses
                .FirstOrDefault(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork)?.Address;

            if (ipv4Address == null)
            {
                return; // could not find an IPv4 address for this adapter
            }

            int ifaceIndex = p.Index;

            logger.LogDebug($"Scanning on iface {adapter.Name}, idx {ifaceIndex}, IP: {ipv4Address}");

            using UdpClient client = new();
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    Socket socket = client.Client;

                    if (socket.IsBound)
                    {
                        continue;
                    }

                    socket.SetSocketOption(
                        SocketOptionLevel.IP,
                        SocketOptionName.MulticastInterface,
                        IPAddress.HostToNetworkOrder(ifaceIndex));

                    client.ExclusiveAddressUse = false;
                    socket.SetSocketOption(
                        SocketOptionLevel.Socket,
                        SocketOptionName.ReuseAddress,
                        true);
                    socket.SetSocketOption(
                        SocketOptionLevel.Socket,
                        SocketOptionName.ReceiveTimeout,
                        (int)scanTime.TotalMilliseconds);
                    client.ExclusiveAddressUse = false;

                    IPEndPoint localEp = new(IPAddress.Any, 5353);
                    logger.LogDebug($"Attempting to bind to {localEp} on adapter {adapter.Name}");

                    socket.Bind(localEp);
                    logger.LogDebug($"Bound to {localEp}");

                    IPAddress multicastAddress = IPAddress.Parse("224.0.0.251");
                    MulticastOption multOpt = new(multicastAddress, ifaceIndex);
                    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, multOpt);
                    logger.LogDebug("Bound to multicast address");

                    // Start a receive loop
                    using CancellationTokenSource tokenSource = new();
                    Task recTask = Task.Run(async () =>
                    {
                        CancellationToken token = tokenSource.Token;
                        try
                        {
                            _ = token.Register(() => ((IDisposable)client).Dispose());
                            while (!token.IsCancellationRequested)
                            {
                                UdpReceiveResult res = await client.ReceiveAsync(tokenSource.Token).ConfigureAwait(false);
                                onResponse(res.RemoteEndPoint.Address, res.Buffer);
                            }
                        }
                        catch when (token.IsCancellationRequested)
                        {
                            // If we're canceling, eat any exceptions that come from here   
                        }
                    }, cancellationToken);

                    IPEndPoint broadcastEp = new(IPAddress.Parse("224.0.0.251"), 5353);
                    logger.LogDebug($"About to send on iface {adapter.Name}");

                    await client.SendAsync(requestBytes, requestBytes.Length, broadcastEp)
                                .ConfigureAwait(false);
                    logger.LogDebug($"Sent mDNS query on iface {adapter.Name}");

                    // wait for responses
                    await Task.Delay(scanTime, cancellationToken)
                              .ConfigureAwait(false);

                    tokenSource.Cancel();
                    logger.LogDebug("Done Scanning");

                    await recTask.ConfigureAwait(false);

                    return;
                }
                catch (Exception e)
                {
                    logger.LogError($"Execption with network request, IP {ipv4Address}\n: {e}");
                    if (i + 1 >= retries) // last one, pass underlying out
                    {
                        // Ensure all inner info is captured                            
                        ExceptionDispatchInfo.Capture(e).Throw();
                        throw;
                    }
                }

                await Task.Delay(retryDelayMilliseconds, cancellationToken).ConfigureAwait(false);
            }
        }

        public Task ListenForAnnouncementsAsync(Action<AdapterInformation, string, byte[]> callback, CancellationToken cancellationToken)
        {
            return Task.WhenAll(
                System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                    .Where(a => a.GetIPProperties().MulticastAddresses.Any()) // Xamarin doesn't support this
                    .Where(a => a.SupportsMulticast)
                    .Where(a => a.OperationalStatus == OperationalStatus.Up)
                    .Where(a => a.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .Where(a => a.GetIPProperties().GetIPv4Properties() != null)
                    .Where(a => a.GetIPProperties().UnicastAddresses.Any(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork))
                    .Select(inter => ListenForAnnouncementsAsync(inter, callback, cancellationToken)));
        }

        private Task ListenForAnnouncementsAsync(System.Net.NetworkInformation.NetworkInterface adapter, Action<AdapterInformation, string, byte[]> callback, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(async () =>
            {
                IPAddress ipv4Address = adapter.GetIPProperties().UnicastAddresses
                .First(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork)?.Address;

                if (ipv4Address == null)
                {
                    return;
                }

                int? ifaceIndex = adapter.GetIPProperties().GetIPv4Properties()?.Index;
                if (ifaceIndex == null)
                {
                    return;
                }

                logger.LogDebug($"Scanning on iface {adapter.Name}, idx {ifaceIndex}, IP: {ipv4Address}");

                using UdpClient client = new();
                Socket socket = client.Client;
                socket.SetSocketOption(
                    SocketOptionLevel.IP,
                    SocketOptionName.MulticastInterface,
                    IPAddress.HostToNetworkOrder(ifaceIndex.Value));

                socket.SetSocketOption(
                    SocketOptionLevel.Socket,
                    SocketOptionName.ReuseAddress,
                    true);
                client.ExclusiveAddressUse = false;


                IPEndPoint localEp = new(IPAddress.Any, 5353);
                socket.Bind(localEp);

                IPAddress multicastAddress = IPAddress.Parse("224.0.0.251");
                MulticastOption multOpt = new(multicastAddress, ifaceIndex.Value);
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, multOpt);

                _ = cancellationToken.Register(() => ((IDisposable)client).Dispose());

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        UdpReceiveResult packet = await client.ReceiveAsync()
                                             .ConfigureAwait(false);
                        try
                        {
                            callback(new AdapterInformation(ipv4Address.ToString(), adapter.Name), packet.RemoteEndPoint.Address.ToString(), packet.Buffer);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError($"Callback threw an exception: {ex}");
                        }
                    }
                    catch when (cancellationToken.IsCancellationRequested)
                    {
                        // eat any exceptions if we've been cancelled
                    }
                }

                logger.LogDebug($"Done listening for mDNS packets on {adapter.Name}, idx {ifaceIndex}, IP: {ipv4Address}.");

                cancellationToken.ThrowIfCancellationRequested();
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
        }
    }
}
