using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace APKInstaller.Helpers
{
    public static class CachesHelper
    {
        public static readonly string TempPath = Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "Caches", $"{Environment.ProcessId}");
        public static readonly string OldTempPath = Path.Combine(Path.GetTempPath(), @"APKInstaller\Caches", $"{Environment.ProcessId}");
    
        public static void CleanCaches(bool isall)
        {
            if (isall)
            {
                if (Directory.Exists(TempPath[..TempPath.LastIndexOf(@"\")]))
                {
                    try { Directory.Delete(TempPath[..TempPath.LastIndexOf(@"\")], true); } catch { }
                }
            }   
            else
            {
                if (Directory.Exists(TempPath))
                {
                    try { Directory.Delete(TempPath, true); } catch { }
                }
            }
        }

        public static void CleanOldCaches(bool isall)
        {
            if (isall)
            {
                if (Directory.Exists(OldTempPath[..OldTempPath.LastIndexOf(@"\")]))
                {
                    try { Directory.Delete(OldTempPath[..OldTempPath.LastIndexOf(@"\")], true); } catch { }
                }
            }
            else
            {
                if (Directory.Exists(OldTempPath))
                {
                    try { Directory.Delete(OldTempPath, true); } catch { }
                }
            }
        }

        public static void CleanAllCaches(bool isall)
        {
            CleanCaches(isall);
            CleanOldCaches(isall);
        }
    }
}
