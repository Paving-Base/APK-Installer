using APKInstaller.Helpers;
using APKInstaller.Pages.AboutPages;
using APKInstaller.Pages.SettingsPages;
using APKInstaller.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InstallPage : Page
    {
        private bool IsCaches;
        internal InstallViewModel Provider;

        public InstallPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (InstallViewModel.Caches != null)
            {
                IsCaches = true;
                Provider = InstallViewModel.Caches;
            }
            else
            {
                IsCaches = false;
                string _path = string.Empty;
                AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();
                switch (args.Kind)
                {
                    case ExtendedActivationKind.File:
                        _path = (args.Data as FileActivatedEventArgs).Files[0].Path;
                        Provider = new InstallViewModel(_path, this);
                        break;
                    case ExtendedActivationKind.ShareTarget:
                        ShareTargetActivatedEventArgs ShareTargetEventArgs = args.Data as ShareTargetActivatedEventArgs;
                        ShareTargetEventArgs.ShareOperation.DismissUI();
                        Provider = new InstallViewModel(string.Empty, this);
                        Provider.OpenAPK(ShareTargetEventArgs.ShareOperation.Data);
                        break;
                    case ExtendedActivationKind.Protocol:
                        ProtocolActivatedEventArgs ProtocolArgs = args.Data as ProtocolActivatedEventArgs;
                        ValueSet ProtocolData = ProtocolArgs.Data;
                        if (ProtocolData == null || ProtocolData.Count == 0)
                        {
                            Provider = new InstallViewModel(ProtocolArgs.Uri, this);
                        }
                        else
                        {
                            if (ProtocolData.ContainsKey("Url"))
                            {
                                Provider = new InstallViewModel(ProtocolData["Url"] as Uri, this);
                            }
                            else if (ProtocolData.ContainsKey("FilePath"))
                            {
                                Provider = new InstallViewModel(ProtocolData["FilePath"] as string, this);
                            }
                        }
                        break;
                    case ExtendedActivationKind.ProtocolForResults:
                        ProtocolForResultsActivatedEventArgs ProtocolForResultsArgs = args.Data as ProtocolForResultsActivatedEventArgs;
                        ValueSet ProtocolForResultsData = ProtocolForResultsArgs.Data;
                        if (ProtocolForResultsData == null || ProtocolForResultsData.Count == 0)
                        {
                            Provider = new InstallViewModel(ProtocolForResultsArgs.Uri, this, ProtocolForResultsArgs.ProtocolForResultsOperation);
                        }
                        else
                        {
                            if (ProtocolForResultsData.ContainsKey("Url"))
                            {
                                Provider = new InstallViewModel(ProtocolForResultsData["Url"] as Uri, this, ProtocolForResultsArgs.ProtocolForResultsOperation);
                            }
                            else if (ProtocolForResultsData.ContainsKey("FilePath"))
                            {
                                Provider = new InstallViewModel(ProtocolForResultsData["FilePath"] as string, this, ProtocolForResultsArgs.ProtocolForResultsOperation);
                            }
                        }
                        break;
                    default:
                        Provider = new InstallViewModel(_path, this);
                        break;
                }
            }
            DataContext = Provider;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Name)
            {
                case "ActionButton" when Provider.IsADBReady:
                    Provider.InstallAPP();
                    break;
                case "ActionButton":
                    _ = Provider.InitADBFile();
                    break;
                case "DownloadButton":
                    Provider.LoadNetAPK();
                    break;
                case "FileSelectButton":
                    Provider.OpenAPK();
                    break;
                case "MoreInfoFlyoutItem":
                    _ = Frame.Navigate(typeof(InfosPage), Provider.ApkInfo);
                    break;
                case "DeviceSelectButton":
                    _ = Frame.Navigate(typeof(SettingsPage));
                    break;
                case "CancelConfirmButton":
                    CancelFlyout.Hide();
                    Provider.CloseAPP();
                    break;
                case "CancelContinueButton":
                    CancelFlyout.Hide();
                    break;
                case "SecondaryActionButton":
                    Provider.OpenAPP();
                    break;
            }
        }

        private async void InitialLoadingUI_Loaded(object sender, RoutedEventArgs e)
        {
            await Provider.Refresh(!IsCaches);
        }

        private void CopyFileItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem element = sender as MenuFlyoutItem;
            DataTransferHelper.CopyFile(element.Tag.ToString(), element.Text);
        }

        private void CopyStringItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem element = sender as MenuFlyoutItem;
            DataTransferHelper.CopyText(element.Tag.ToString(), element.Text);
        }

        private void CopyBitmapItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem element = sender as MenuFlyoutItem;
            DataTransferHelper.CopyBitmap(element.Tag.ToString(), element.Text);
        }

        private void ShareFileItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem element = sender as MenuFlyoutItem;
            DataTransferHelper.ShareFile(element.Tag.ToString(), element.Text, element.Tag.ToString());
        }

        private void ShareUrlItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem element = sender as MenuFlyoutItem;
            DataTransferHelper.ShareURL(new Uri(element.Tag.ToString()), element.Text, element.Tag.ToString());
        }

        private void Page_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private void Page_Drop(object sender, DragEventArgs e)
        {
            Provider.OpenAPK(e.DataView);
            e.Handled = true;
        }
    }
}
