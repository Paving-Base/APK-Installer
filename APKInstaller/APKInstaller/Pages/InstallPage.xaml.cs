using APKInstaller.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppLifecycle;
using System.Linq;
using Windows.ApplicationModel.Activation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InstallPage : Page
    {
        internal InstallViewModel Provider;

        public InstallPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
#if !DEBUG
            string _path = string.Empty;
#else
            string _path = @"C:\Users\qq251\Downloads\Programs\Minecraft_1.17.40.06_sign.apk";
#endif
            AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();
            switch (args.Kind)
            {
                case ExtendedActivationKind.File:
                    _path = (args.Data as IFileActivatedEventArgs).Files.First().Path;
                    break;
                default:
                    break;
            }
            Provider = new InstallViewModel(_path, this);
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
                case "SecondaryActionButton":
                    Provider.OpenAPP();
                    break;
                case "CancelOperationButton":
                    Application.Current.Exit();
                    break;
            }
        }

        private async void InitialLoadingUI_Loaded(object sender, RoutedEventArgs e)
        {
            await Provider.Refresh();
        }
    }
}
