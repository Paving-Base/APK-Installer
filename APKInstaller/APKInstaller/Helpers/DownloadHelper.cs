using Downloader;

namespace APKInstaller.Helpers
{
    public static class DownloadHelper
    {
        public static DownloadConfiguration Configuration;

        static DownloadHelper()
        {
            Configuration = new DownloadConfiguration
            {
                ChunkCount = 8,
                ParallelDownload = true,
                TempDirectory = CachesHelper.TempPath
            };
        }
    }
}
