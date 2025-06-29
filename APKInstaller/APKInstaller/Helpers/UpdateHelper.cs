using APKInstaller.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace APKInstaller.Helpers
{
    public static class UpdateHelper
    {
        private const string KKPP_API = "https://v2.kkpp.cc/repos/{0}/{1}/releases/latest";
        private const string GITHUB_API = "https://api.github.com/repos/{0}/{1}/releases/latest";

        public static Task<UpdateInfo> CheckUpdateAsync(string username, string repository)
        {
            PackageVersion currentVersion = Package.Current.Id.Version;
            return CheckUpdateAsync(username, repository, currentVersion);
        }

        public static async Task<UpdateInfo> CheckUpdateAsync(string username, string repository, PackageVersion currentVersion)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (string.IsNullOrEmpty(repository))
            {
                throw new ArgumentNullException(nameof(repository));
            }

            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            HttpClient client = new();
            client.DefaultRequestHeaders.Add("User-Agent", username);
            string url = string.Format(GITHUB_API, username, repository);
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            UpdateInfo result = JsonSerializer.Deserialize<UpdateInfo>(responseBody, SourceGenerationContext.Default.UpdateInfo);

            if (result != null)
            {
                SystemVersionInfo newVersionInfo = GetAsVersionInfo(result.TagName);
                int major = currentVersion.Major <= 0 ? 0 : currentVersion.Major;
                int minor = currentVersion.Minor <= 0 ? 0 : currentVersion.Minor;
                int build = currentVersion.Build <= 0 ? 0 : currentVersion.Build;
                int revision = currentVersion.Revision <= 0 ? 0 : currentVersion.Revision;

                SystemVersionInfo currentVersionInfo = new(major, minor, build, revision);

                result.IsExistNewVersion = newVersionInfo > currentVersionInfo;

                return result;
            }

            return null;
        }

        private static SystemVersionInfo GetAsVersionInfo(string version)
        {
            int[] numbs = [.. GetVersionNumbers(version).Split('.').Select(int.Parse)];
            return numbs.Length <= 1
                ? new SystemVersionInfo(numbs[0], 0, 0, 0)
                : numbs.Length <= 2
                    ? new SystemVersionInfo(numbs[0], numbs[1], 0, 0)
                    : numbs.Length <= 3
                        ? new SystemVersionInfo(numbs[0], numbs[1], numbs[2], 0)
                        : new SystemVersionInfo(numbs[0], numbs[1], numbs[2], numbs[3]);
        }

        private static string GetVersionNumbers(string version)
        {
            string allowedChars = "01234567890.";
            return new string([.. version.Where(allowedChars.Contains)]);
        }
    }
}
