using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Zeroconf.DNS;
using Zeroconf.Interfaces;
using Zeroconf.Models;

namespace Zeroconf
{
    public static partial class ZeroconfResolver
    {
        /// <summary>
        /// Resolves available ZeroConf services
        /// </summary>
        /// <param name="scanTime">Default is 2 seconds</param>
        /// <param name="cancellationToken"></param>
        /// <param name="protocol"></param>
        /// <param name="retries">If the socket is busy, the number of times the resolver should retry</param>
        /// <param name="retryDelayMilliseconds">The delay time between retries</param>
        /// <param name="callback">Called per record returned as they come in.</param>
        /// <param name="netInterfacesToSendRequestOn">The network interfaces/adapters to use. Use all if null</param>
        /// <returns></returns>
        public static Task<IReadOnlyList<IZeroconfHost>> ResolveAsync(
            string protocol,
            TimeSpan scanTime = default,
            int retries = 2,
            int retryDelayMilliseconds = 2000,
            Action<IZeroconfHost> callback = null,
            NetworkInterface[] netInterfacesToSendRequestOn = null,
            CancellationToken cancellationToken = default)
        {
            return string.IsNullOrWhiteSpace(protocol)
                ? throw new ArgumentNullException(nameof(protocol))
                : ResolveAsync(
                    new[] { protocol },
                    scanTime,
                    retries,
                    retryDelayMilliseconds,
                    callback,
                    netInterfacesToSendRequestOn,
                    cancellationToken);
        }

        /// <summary>
        /// Resolves available ZeroConf services
        /// </summary>
        /// <param name="scanTime">Default is 2 seconds</param>
        /// <param name="cancellationToken"></param>
        /// <param name="protocols"></param>
        /// <param name="retries">If the socket is busy, the number of times the resolver should retry</param>
        /// <param name="retryDelayMilliseconds">The delay time between retries</param>
        /// <param name="callback">Called per record returned as they come in.</param>
        /// <param name="netInterfacesToSendRequestOn">The network interfaces/adapters to use. Use all if null</param>
        /// <returns></returns>
        public static async Task<IReadOnlyList<IZeroconfHost>> ResolveAsync(
            IEnumerable<string> protocols,
            TimeSpan scanTime = default,
            int retries = 2,
            int retryDelayMilliseconds = 2000,
            Action<IZeroconfHost> callback = null,
            NetworkInterface[] netInterfacesToSendRequestOn = null,
            CancellationToken cancellationToken = default)
        {
            if (retries <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retries));
            }

            if (retryDelayMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retryDelayMilliseconds));
            }

            if (scanTime == default)
            {
                scanTime = TimeSpan.FromSeconds(2);
            }

            ResolveOptions options = new(protocols)
            {
                Retries = retries,
                RetryDelay = TimeSpan.FromMilliseconds(retryDelayMilliseconds),
                ScanTime = scanTime
            };

            return await ResolveAsync(options, callback, netInterfacesToSendRequestOn, cancellationToken).ConfigureAwait(false);
        }


        /// <summary>
        /// Resolves available ZeroConf services
        /// </summary>
        /// <param name="options"></param>
        /// <param name="callback">Called per record returned as they come in.</param>
        /// <param name="netInterfacesToSendRequestOn">The network interfaces/adapters to use. Use all if null</param>
        /// <returns></returns>
        public static async Task<IReadOnlyList<IZeroconfHost>> ResolveAsync(
            ResolveOptions options,
            Action<IZeroconfHost> callback = null,
            NetworkInterface[] netInterfacesToSendRequestOn = null,
            CancellationToken cancellationToken = default)
        {
            return options == null
                ? throw new ArgumentNullException(nameof(options))
                : await ResolveAsyncOriginal(options, callback, netInterfacesToSendRequestOn, cancellationToken);
        }

        internal static async Task<IReadOnlyList<IZeroconfHost>> ResolveAsyncOriginal(
            ResolveOptions options,
            Action<IZeroconfHost> callback = null,
            NetworkInterface[] netInterfacesToSendRequestOn = null,
            CancellationToken cancellationToken = default)
        {
            Action<string, Response> wrappedAction = null;

            if (callback != null)
            {
                wrappedAction = (address, resp) =>
                {
                    ZeroconfHost zc = ResponseToZeroconf(resp, address, options);
                    if (zc.Services.Any(s => options.Protocols.Contains(s.Value.Name)))
                    {
                        callback(zc);
                    }
                };
            }

            IDictionary<string, Response> dict = await ResolveInternal(
                options,
                wrappedAction,
                cancellationToken,
                netInterfacesToSendRequestOn).ConfigureAwait(false);

            return dict.Select(
                pair => ResponseToZeroconf(pair.Value, pair.Key, options))
                    .Where(zh => zh.Services.Any(s => options.Protocols.Contains(s.Value.Name))) // Ensure we only return records that have matching services
                    .ToList();
        }


        /// <summary>
        /// Returns all available domains with services on them
        /// </summary>
        /// <param name="scanTime">Default is 2 seconds</param>
        /// <param name="cancellationToken"></param>
        /// <param name="retries">If the socket is busy, the number of times the resolver should retry</param>
        /// <param name="retryDelayMilliseconds">The delay time between retries</param>
        /// <param name="callback">Called per record returned as they come in.</param>
        /// <param name="netInterfacesToSendRequestOn">The network interfaces/adapters to use. Use all if null</param>
        /// <returns></returns>
        public static async Task<ILookup<string, string>> BrowseDomainsAsync(
            TimeSpan scanTime = default,
            int retries = 2,
            int retryDelayMilliseconds = 2000,
            Action<string, string> callback = null,
            NetworkInterface[] netInterfacesToSendRequestOn = null,
            CancellationToken cancellationToken = default)

        {
            if (retries <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retries));
            }

            if (retryDelayMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retryDelayMilliseconds));
            }

            if (scanTime == default)
            {
                scanTime = TimeSpan.FromSeconds(2);
            }

            BrowseDomainsOptions options = new()
            {
                Retries = retries,
                RetryDelay = TimeSpan.FromMilliseconds(retryDelayMilliseconds),
                ScanTime = scanTime
            };

            return await BrowseDomainsAsync(options, callback, netInterfacesToSendRequestOn, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Returns all available domains with services on them
        /// </summary>
        /// <param name="options"></param>
        /// <param name="callback">Called per record returned as they come in.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="netInterfacesToSendRequestOn">The network interfaces/adapters to use. Use all if null</param>
        /// <returns></returns>
        public static async Task<ILookup<string, string>> BrowseDomainsAsync(
            BrowseDomainsOptions options,
            Action<string, string> callback = null,
            NetworkInterface[] netInterfacesToSendRequestOn = null,
            CancellationToken cancellationToken = default)
        {
            return options == null
                ? throw new ArgumentNullException(nameof(options))
                : await BrowseDomainsAsyncOriginal(options, callback, netInterfacesToSendRequestOn, cancellationToken);
        }

        internal static async Task<ILookup<string, string>> BrowseDomainsAsyncOriginal(
            BrowseDomainsOptions options,
            Action<string, string> callback = null,
            NetworkInterface[] netInterfacesToSendRequestOn = null,
            CancellationToken cancellationToken = default)
        {
            Action<string, Response> wrappedAction = null;
            if (callback != null)
            {
                wrappedAction = (address, response) =>
                {
                    foreach (string service in BrowseResponseParser(response))
                    {
                        callback(service, address);
                    }
                };
            }

            IDictionary<string, Response> dict = await ResolveInternal(
                options,
                wrappedAction,
                cancellationToken,
                netInterfacesToSendRequestOn).ConfigureAwait(false);

            var r = from kvp in dict
                    from service in BrowseResponseParser(kvp.Value)
                    select new { Service = service, Address = kvp.Key };

            return r.ToLookup(k => k.Service, k => k.Address);
        }

        /// <summary>
        /// Listens for mDNS Service Announcements
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task ListenForAnnouncementsAsync(Action<ServiceAnnouncement> callback, CancellationToken cancellationToken)
        {
            return NetworkInterface.ListenForAnnouncementsAsync((adapter, address, buffer) =>
            {
                Response response = new(buffer);
                if (response.IsQueryResponse)
                {
                    callback(new ServiceAnnouncement(adapter, ResponseToZeroconf(response, address, null)));
                }
            }, cancellationToken);
        }
    }
}
