using APKInstaller.Helpers;
using CommunityToolkit.WinUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;

namespace APKInstaller.Helper
{
    /// <summary>
    /// Class providing functionality to support generating and copying protocol activation URIs.
    /// </summary>
    public static class ClipboardHelper
    {
        private static DataPackage GetTextDataPackage(string text, string title, string description)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            dataPackage.Properties.Title = title;
            dataPackage.Properties.Description = description;
            return dataPackage;
        }

        private static async Task<DataPackage> GetFileDataPackage(string filePath, string fileName, string description)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);

            IEnumerable<IStorageFile> files = new List<StorageFile> { file };

            var dataPackage = new DataPackage();
            dataPackage.SetStorageItems(files);
            dataPackage.Properties.Title = fileName;
            dataPackage.Properties.Description = description;

            return dataPackage;
        }

        private static async Task<DataPackage> GetBitmapDataPackage(string bitmapPath, string bitmapName, string description)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(bitmapPath);

            RandomAccessStreamReference bitmap = RandomAccessStreamReference.CreateFromFile(file);

            var dataPackage = new DataPackage();
            dataPackage.SetBitmap(bitmap);
            dataPackage.Properties.Title = bitmapName;
            dataPackage.Properties.Description = description;

            return dataPackage;
        }

        public static void Copy(this DataPackage dataPackage) => Clipboard.SetContentWithOptions(dataPackage, null);

        public static void CopyText(string text, string title, string description = null)
        {
            var dataPackage = GetTextDataPackage(text, title, description);
            dataPackage.Copy();
        }

        public static async void CopyFile(string filePath, string fileName, string description = null)
        {
            var dataPackage = await GetFileDataPackage(filePath, fileName, description);
            dataPackage.Copy();
        }

        public static async void CopyBitmap(string bitmapPath, string bitmapName, string description = null)
        {
            var dataPackage = await GetFileDataPackage(bitmapPath, bitmapName, description);
            dataPackage.Copy();
        }

        public static void Share(this DataPackage dataPackage)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();

            void On_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
            {
                dataTransferManager.DataRequested -= On_DataRequested;
                DataRequest request = args.Request;
                request.Data = dataPackage;
            }

            dataTransferManager.DataRequested += On_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        public static async void ShareURL(string filePath, string fileName, string description = null)
        {
            var dataPackage = await GetFileDataPackage(filePath, fileName, description);
            dataPackage.Share();
        }

        public static async void ShareFile(string filePath, string fileName, string description = null)
        {
            var dataPackage = await GetFileDataPackage(filePath, fileName, description);
            dataPackage.Share();
        }
    }
}
