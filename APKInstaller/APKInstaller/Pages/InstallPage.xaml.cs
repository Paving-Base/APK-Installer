using APKInstaller.Helper;
using APKInstaller.Pages.SettingsPages;
using APKInstaller.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Linq;
using Windows.ApplicationModel.Activation;
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
                        _path = (args.Data as FileActivatedEventArgs).Files.First().Path;
                        Provider = new InstallViewModel(_path, this);
                        break;
                    case ExtendedActivationKind.Protocol:
                        ProtocolActivatedEventArgs ProtocolArgs = args.Data as ProtocolActivatedEventArgs;
                        ValueSet ProtocolData = ProtocolArgs.Data;
                        if (ProtocolData.Count <= 0)
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
                        if (ProtocolForResultsData.Count <= 0)
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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Provider.Dispose();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Name)
            {
                case "ActionButton":
                    Provider.InstallAPP();
                    break;
                case "DownloadButton":
                    Provider.LoadNetAPK();
                    break;
                case "FileSelectButton":
                    Provider.OpenAPK();
                    break;
                case "DeviceSelectButton":
                    Frame.Navigate(typeof(SettingsPage));
                    break;
                case "SecondaryActionButton":
                    Provider.OpenAPP();
                    break;
                case "CancelOperationButton":
                    Provider.CloseAPP();
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
            ClipboardHelper.CopyFile(element.Tag.ToString(), element.Text);
        }

        private void CopyStringItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem element = sender as MenuFlyoutItem;
            ClipboardHelper.CopyText(element.Tag.ToString(), element.Text);
        }

        private void CopyBitmapItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem element = sender as MenuFlyoutItem;
            ClipboardHelper.CopyBitmap(element.Tag.ToString(), element.Text);
        }

        private void ShareFileItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem element = sender as MenuFlyoutItem;
            ClipboardHelper.ShareFile(element.Tag.ToString(), element.Text);
        }
    }
}
