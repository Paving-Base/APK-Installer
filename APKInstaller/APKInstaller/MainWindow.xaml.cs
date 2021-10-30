using AdvancedSharpAdbClient;
using APKInstaller.Helpers;
using APKInstaller.Pages;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UIHelper.MainWindow = this;
            MainPage MainPage = new();
            Content = MainPage;
        }

        private void Window_Closed(object sender, WindowEventArgs args) => new AdvancedAdbClient().KillAdb();
    }
}
