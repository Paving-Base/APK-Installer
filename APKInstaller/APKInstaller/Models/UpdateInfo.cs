using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace APKInstaller.Models
{
    public class UpdateInfo
    {
        [JsonPropertyName("url")]
        public string ApiUrl { get; set; }
        [JsonPropertyName("html_url")]
        public string ReleaseUrl { get; set; }
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; }
        [JsonPropertyName("prerelease")]
        public bool IsPreRelease { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("published_at")]
        public DateTime PublishedAt { get; set; }
        [JsonPropertyName("assets")]
        public List<Asset> Assets { get; set; }
        [JsonPropertyName("body")]
        public string Changelog { get; set; }
        public bool IsExistNewVersion { get; set; }
    }

    public class Asset
    {
        [JsonPropertyName("size")]
        public int Size { get; set; }
        [JsonPropertyName("browser_download_url")]
        public string Url { get; set; }
    }
}
