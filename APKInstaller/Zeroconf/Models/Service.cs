using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeroconf.Interfaces;

namespace Zeroconf.Models
{
    internal class Service : IService
    {
        private readonly List<IReadOnlyDictionary<string, string>> properties = new();

        public string Name { get; set; }
        public string ServiceName { get; set; }
        public int Port { get; set; }
        public int Ttl { get; set; }

        public IReadOnlyList<IReadOnlyDictionary<string, string>> Properties => properties;

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.Append($"\t| Service: {Name}\n\t| ServiceName: {ServiceName}\n\t| Port: {Port}\n\t| TTL: {Ttl}\n\t| PropertySets: {properties.Count}");

            if (properties.Any())
            {
                sb.AppendLine();
                for (int i = 0; i < properties.Count; i++)
                {
                    sb.AppendLine("\t\t| -------------------");
                    sb.Append($"\t\t| Property Set #{i}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t| -------------------");

                    foreach (KeyValuePair<string, string> kvp in properties[i])
                    {
                        sb.AppendLine($"\t\t| {kvp.Key} = {kvp.Value}");
                    }
                    sb.Append("\t\t| -------------------");
                }
            }

            return sb.ToString();
        }

        internal void AddPropertySet(IReadOnlyDictionary<string, string> set)
        {
            if (set == null)
            {
                throw new ArgumentNullException(nameof(set));
            }

            properties.Add(set);
        }
    }
}
