using System;
using System.Collections.Generic;
using System.Threading;
using Zeroconf.Interfaces;
using Zeroconf.Models;

namespace Zeroconf
{
    public class ResolverListener : IDisposable
    {
        private readonly IEnumerable<string> protocols;
        private readonly TimeSpan scanTime;
        private readonly int retries;
        private readonly int retryDelayMilliseconds;
        private readonly Timer timer;
        private readonly int pingsUntilRemove;
        private HashSet<(string, string)> discoveredHosts = new();
        private readonly IDictionary<(string, string), int> toRemove = new Dictionary<(string, string), int>();

        public IReadOnlyList<IZeroconfHost> Hosts { get; private set; }

        public event EventHandler<IZeroconfHost> ServiceFound;
        public event EventHandler<IZeroconfHost> ServiceLost;
        public event EventHandler<Exception> Error;

        internal ResolverListener(IEnumerable<string> protocols, int queryInterval, int pingsUntilRemove, TimeSpan scanTime, int retries, int retryDelayMilliseconds)
        {
            this.protocols = protocols;
            this.scanTime = scanTime;
            this.retries = retries;
            this.retryDelayMilliseconds = retryDelayMilliseconds;
            this.pingsUntilRemove = pingsUntilRemove;
            timer = new Timer(DiscoverHosts, this, 0, queryInterval);
        }

        private async void DiscoverHosts(object state)
        {
            try
            {
                ResolverListener instance = state as ResolverListener;
                IReadOnlyList<IZeroconfHost> hosts = await ZeroconfResolver.ResolveAsync(protocols, scanTime, retries, retryDelayMilliseconds).ConfigureAwait(false);
                instance.OnResolved(hosts);
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
            }
        }

        private void OnResolved(IReadOnlyList<IZeroconfHost> hosts)
        {
            Hosts = hosts;
            lock (discoveredHosts)
            {
                HashSet<(string, string)> newHosts = new(discoveredHosts);
                HashSet<(string, string)> remainingHosts = new(discoveredHosts);

                foreach (IZeroconfHost host in hosts)
                {
                    foreach (KeyValuePair<string, IService> service in host.Services)
                    {
                        (string, string) keyValue = (host.DisplayName, service.Value.Name);
                        if (discoveredHosts.Contains(keyValue))
                        {
                            remainingHosts.Remove(keyValue);
                        }
                        else
                        {
                            ServiceFound?.Invoke(this, host);
                            newHosts.Add(keyValue);
                        }
                        if (toRemove.ContainsKey(keyValue))
                        {
                            toRemove.Remove(keyValue);
                        }
                    }
                }

                foreach ((string, string) service in remainingHosts)
                {
                    if (toRemove.ContainsKey(service))
                    {
                        //zeroconf sometimes reports missing hosts incorrectly. 
                        //after pingsUntilRemove missing hosts reports, we'll remove the service from the list.
                        if (++toRemove[service] > pingsUntilRemove)
                        {
                            toRemove.Remove(service);
                            newHosts.Remove(service);
                            ServiceLost?.Invoke(this, new ZeroconfHost { DisplayName = service.Item1, Id = service.Item2 });
                        }
                    }
                    else
                    {
                        toRemove.Add(service, 0);
                    }
                }

                discoveredHosts = newHosts;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer?.Dispose();
            }
        }
    }
}
