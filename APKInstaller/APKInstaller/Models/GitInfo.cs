using APKInstaller.Helpers;
using System.Text.RegularExpressions;

namespace APKInstaller.Models
{
    public class GitInfo
    {
        public static string NUAA_API = "https://raw.nuaa.cf/{0}/{1}/{2}/{3}/{4}";
        public static string YZUU_API = "https://raw.yzuu.cf/{0}/{1}/{2}/{3}/{4}";
        public static string FASTGIT_API = "https://raw.fastgit.org/{0}/{1}/{2}/{3}/{4}";
        public static string JSDELIVR_API = "https://cdn.jsdelivr.net/gh/{0}/{1}@{2}/{3}/{4}";
        public static string GITHUB_API = "https://raw.githubusercontent.com/{0}/{1}/{2}/{3}/{4}";

        public string Path { get; private set; }
        public string Branch { get; private set; }
        public string UserName { get; private set; }
        public string FileName { get; private set; }
        public string Repository { get; private set; }

        public GitInfo(string username, string repository, string branch, string path, string filename)
        {
            Path = path;
            Branch = branch;
            UserName = username;
            FileName = filename;
            Repository = repository;
        }

        public string FormatURL(string API, bool local = true)
        {
            if (local)
            {
                string Culture = LanguageHelper.GetCurrentLanguage();
                return string.Format(API, UserName, Repository, Branch, Path, AddLanguage(FileName, Culture));
            }
            return string.Format(API, UserName, Repository, Branch, Path, FileName);
        }

        private string AddLanguage(string filename, string langcode)
        {
            Regex file = new(@"^.*(\.\w+)$");
            Regex lang = new(@"^.*\.[a-z]{2}(-[A-Z]{2})?\.\w+$");
            return file.IsMatch(filename) && !lang.IsMatch(filename)
                ? Regex.Replace(filename, @"(?<name>.*)(?<extension>\.\w+$)", $"${{name}}.{langcode}${{extension}}")
                : filename;
        }
    }
}
