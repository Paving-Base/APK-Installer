using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using WinRT;
using WinRT.Interop;

namespace APKInstaller.Helpers
{
    /// <summary>
    /// Class providing functionality to support generating and copying protocol activation URIs.
    /// </summary>
    public static class DataTransferHelper
    {
        [ComImport]
        [Guid("3A3DCD6C-3EAB-43DC-BCDE-45671CE800C8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDataTransferManagerInterop
        {
            IntPtr GetForWindow([In] IntPtr appWindow, [In] ref Guid riid);
            void ShowShareUIForWindow(IntPtr appWindow);
        }

        private static readonly Guid _dtm_iid = new(0xa5caee9b, 0x8708, 0x49d1, 0x8d, 0x36, 0x67, 0xd2, 0x5a, 0x8d, 0xa0, 0x0c);

        private static DataPackage GetTextDataPackage(string text, string title, string description)
        {
            DataPackage dataPackage = new();
            dataPackage.SetText(text);
            if (title != null) { dataPackage.Properties.Title = title; }
            if (description != null) { dataPackage.Properties.Description = description; }
            return dataPackage;
        }

        private static async Task<DataPackage> GetFileDataPackage(string filePath, string fileName, string description)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);

            IEnumerable<IStorageFile> files = new List<StorageFile> { file };

            DataPackage dataPackage = new();
            dataPackage.SetStorageItems(files);
            if (fileName != null) { dataPackage.Properties.Title = fileName; }
            if (description != null) { dataPackage.Properties.Description = description; }

            return dataPackage;
        }

        private static DataPackage GetUrlDataPackage(Uri uri, string displayName, string description)
        {
            string htmlFormat = HtmlFormatHelper.CreateHtmlFormat($"<a href='{uri}'>{displayName}</a>");

            DataPackage dataPackage = new();
            dataPackage.SetWebLink(uri);
            dataPackage.SetText(uri.ToString());
            dataPackage.SetHtmlFormat(htmlFormat);
            if (displayName != null) { dataPackage.Properties.Title = displayName; }
            if (description != null) { dataPackage.Properties.Description = description; }

            return dataPackage;
        }

        private static async Task<DataPackage> GetBitmapDataPackage(string bitmapPath, string bitmapName, string description)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(bitmapPath);

            RandomAccessStreamReference bitmap = RandomAccessStreamReference.CreateFromFile(file);

            DataPackage dataPackage = new();
            dataPackage.SetBitmap(bitmap);
            if (bitmapName != null) { dataPackage.Properties.Title = bitmapName; }
            if (description != null) { dataPackage.Properties.Description = description; }

            return dataPackage;
        }

        public static void Copy(this DataPackage dataPackage) => Clipboard.SetContentWithOptions(dataPackage, null);

        public static void CopyText(string text, string title, string description = null)
        {
            DataPackage dataPackage = GetTextDataPackage(text, title, description);
            dataPackage.Copy();
        }

        public static async void CopyFile(string filePath, string fileName, string description = null)
        {
            DataPackage dataPackage = await GetFileDataPackage(filePath, fileName, description);
            dataPackage.Copy();
        }

        public static async void CopyBitmap(string bitmapPath, string bitmapName, string description = null)
        {
            DataPackage dataPackage = await GetFileDataPackage(bitmapPath, bitmapName, description);
            dataPackage.Copy();
        }

        public static void Share(this DataPackage dataPackage)
        {
            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            IntPtr hWnd = WindowNative.GetWindowHandle(UIHelper.MainWindow);

            IDataTransferManagerInterop interop = DataTransferManager.As<IDataTransferManagerInterop>();

            IntPtr result = interop.GetForWindow(hWnd, _dtm_iid);
            DataTransferManager dataTransferManager = MarshalInterface<DataTransferManager>.FromAbi(result);

            dataTransferManager.DataRequested += (sender, args) => { args.Request.Data = dataPackage; };

            // Show the Share UI
            interop.ShowShareUIForWindow(hWnd);
        }

        public static void ShareURL(Uri url, string displayName, string description = null)
        {
            DataPackage dataPackage = GetUrlDataPackage(url, displayName, description);
            dataPackage.Share();
        }

        public static async void ShareFile(string filePath, string fileName, string description = null)
        {
            DataPackage dataPackage = await GetFileDataPackage(filePath, fileName, description);
            dataPackage.Share();
        }
    }
}
