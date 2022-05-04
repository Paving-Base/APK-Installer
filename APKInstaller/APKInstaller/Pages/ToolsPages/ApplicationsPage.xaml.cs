﻿using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using APKInstaller.Controls;
using APKInstaller.Helpers;
using APKInstaller.ViewModels.ToolsPages;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Pages.ToolsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ApplicationsPage : Page
    {
        private ApplicationsViewModel Provider;

        public ApplicationsPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Provider = new ApplicationsViewModel(this);
            DataContext = Provider;
            Provider.TitleBar = TitleBar;
            ADBHelper.Monitor.DeviceChanged += OnDeviceChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ADBHelper.Monitor.DeviceChanged -= OnDeviceChanged;
        }

        private void OnDeviceChanged(object sender, DeviceDataEventArgs e) => _ = DispatcherQueue.EnqueueAsync(Provider.GetDevices);

        private void TitleBar_BackRequested(TitleBar sender, object e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ = Provider.GetApps();
        }

        private void TitleBar_RefreshEvent(TitleBar sender, object e)
        {
            _ = Provider.GetDevices().ContinueWith((Task) => _ = Provider.GetApps());
        }

        private void ListViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Button Button = (sender as FrameworkElement).FindDescendant<Button>();
            MenuFlyout Flyout = (MenuFlyout)Button.Flyout;
            Flyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement Button = sender as FrameworkElement;
            switch (Button.Name)
            {
                case "Stop":
                    new AdvancedAdbClient().StopApp(Provider.devices[DeviceComboBox.SelectedIndex], Button.Tag.ToString());
                    break;
                case "Start":
                    new AdvancedAdbClient().StartApp(Provider.devices[DeviceComboBox.SelectedIndex], Button.Tag.ToString());
                    break;
                case "Uninstall":
                    break;
            }
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            ListView ListView = sender as ListView;
            ItemsStackPanel StackPanel = ListView.FindDescendant<ItemsStackPanel>();
            if (StackPanel != null)
            {
                StackPanel.Margin = new Thickness(14, 16, 14, 16);
            }
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            Provider.DeviceComboBox = sender as ComboBox;
            _ = Provider.GetDevices();
        }
    }
}
