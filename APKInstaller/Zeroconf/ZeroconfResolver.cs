using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Zeroconf.Common;
using Zeroconf.DNS;
using Zeroconf.Interfaces;
using Zeroconf.Models;

namespace Zeroconf
{
    /// <summary>
    /// Looks for ZeroConf devices
    /// </summary>
    public static partial class ZeroconfResolver
    {
        /// <summary>
        /// The logger to use when logging messages.
        /// </summary>
        private static readonly ILogger logger = NullLogger.Instance;

        private static readonly AsyncLock ResolverLock = new();
        private static readonly INetworkInterface NetworkInterface = new NetworkInterface();

        private static IEnumerable<string> BrowseResponseParser(Response response)
        {
            return response.RecordsPTR.Select(ptr => ptr.PTRDNAME);
        }

        private static async Task<IDictionary<string, Response>> ResolveInternal(
            ZeroconfOptions options,
            Action<string, Response> callback,
            CancellationToken cancellationToken,
            System.Net.NetworkInformation.NetworkInterface[] netInterfacesToSendRequestOn = null)
        {
            byte[] requestBytes = GetRequestBytes(options);

            using IDisposable disposable = options.AllowOverlappedQueries
                ? new NullDisposable()
                : await ResolverLock.LockAsync();

            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, Response> dict = [];

            void Converter(IPAddress address, byte[] buffer)
            {
                Response resp = new(buffer);
                RecordPTR firstPtr = resp.RecordsPTR.FirstOrDefault();
                string name = firstPtr?.PTRDNAME.Split('.')[0] ?? string.Empty;
                string addrString = address.ToString();

                logger.LogDebug($"IP: {addrString}, {(string.IsNullOrEmpty(name) ? string.Empty : $"Name: {name}, ")}Bytes: {buffer.Length}, IsResponse: {resp.header.QR}");

                if (resp.header.QR)
                {
                    string key = $"{addrString}{(string.IsNullOrEmpty(name) ? "" : $": {name}")}";
                    lock (dict)
                    {
                        dict[key] = resp;
                    }

                    callback?.Invoke(key, resp);
                }
            }

            logger.LogDebug($"Looking for {string.Join(", ", options.Protocols)} with scantime {options.ScanTime}");

            await NetworkInterface.NetworkRequestAsync(
                requestBytes,
                options.ScanTime,
                options.Retries,
                (int)options.RetryDelay.TotalMilliseconds,
                Converter,
                cancellationToken,
                netInterfacesToSendRequestOn).ConfigureAwait(false);

            return dict;
        }

        private static byte[] GetRequestBytes(ZeroconfOptions options)
        {
            Request req = new();
            QType queryType = options.ScanQueryType == ScanQueryType.Ptr ? QType.PTR : QType.ANY;

            foreach (string protocol in options.Protocols)
            {
                Question question = new(protocol, queryType, QClass.IN);

                req.AddQuestion(question);
            }

            return req.Data;
        }

        private static ZeroconfHost ResponseToZeroconf(Response response, string remoteAddress, ResolveOptions options)
        {
            List<string> ipv4Adresses = response.Answers
                .Select(r => r.RECORD)
                .OfType<RecordA>()
                .Concat(
                    response.Additionals
                    .Select(r => r.RECORD)
                    .OfType<RecordA>())
                .Select(aRecord => aRecord.Address)
                .Distinct()
                .ToList();

            List<string> ipv6Adresses = response.Answers
                .Select(r => r.RECORD)
                .OfType<RecordAAAA>()
                .Concat(
                    response.Additionals
                    .Select(r => r.RECORD)
                    .OfType<RecordAAAA>())
                .Select(aRecord => aRecord.Address)
                .Distinct()
                .ToList();

            ZeroconfHost z = new()
            {
                IPAddresses = ipv4Adresses.Concat(ipv6Adresses).ToList()
            };

            z.Id = z.IPAddresses.FirstOrDefault() ?? remoteAddress;

            bool dispNameSet = false;

            foreach (RecordPTR ptrRec in response.RecordsPTR)
            {
                // set the display name if needed
                if (!dispNameSet
                    && (options == null
                        || (options != null
                            && options.Protocols.Contains(ptrRec.RR.NAME))))
                {
                    z.DisplayName = ptrRec.PTRDNAME.Replace($".{ptrRec.RR.NAME}", "");
                    dispNameSet = true;
                }

                // Get the matching service records
                List<Record> responseRecords = response.RecordsRR
                    .Where(r => r.NAME == ptrRec.PTRDNAME)
                    .Select(r => r.RECORD)
                    .ToList();

                RecordSRV srvRec = responseRecords.OfType<RecordSRV>().FirstOrDefault();
                if (srvRec == null)
                {
                    continue; // Missing the SRV record, not valid
                }

                Service svc = new()
                {
                    Name = ptrRec.RR.NAME,
                    ServiceName = srvRec.RR.NAME,
                    Port = srvRec.PORT,
                    Ttl = (int)srvRec.RR.TTL,
                };

                // There may be 0 or more text records - property sets
                foreach (RecordTXT txtRec in responseRecords.OfType<RecordTXT>())
                {
                    Dictionary<string, string> set = [];
                    foreach (string txt in txtRec.TXT)
                    {
                        string[] split = txt.Split(new[] { '=' }, 2);
                        if (split.Length == 1)
                        {
                            if (!string.IsNullOrWhiteSpace(split[0]))
                            {
                                set[split[0]] = null;
                            }
                        }
                        else
                        {
                            set[split[0]] = split[1];
                        }
                    }
                    svc.AddPropertySet(set);
                }
                z.AddService(svc);
            }
            return z;
        }
    }
}
