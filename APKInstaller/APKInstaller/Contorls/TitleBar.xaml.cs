using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Contorls
{
    public sealed partial class TitleBar : UserControl
    {
        public bool IsBackButtonEnabled { get => BackButton.IsEnabled; set => BackButton.IsEnabled = value; }

        public string Title
        {
            get => TitleBlock.Text;
            set => TitleBlock.Text = value ?? string.Empty;
        }

        public event RoutedEventHandler RefreshEvent;
        public event RoutedEventHandler BackRequested;

        public Visibility BackButtonVisibility { get => BackButton.Visibility; set => BackButton.Visibility = value; }
        public Visibility RefreshButtonVisibility { get => RefreshButton.Visibility; set => RefreshButton.Visibility = value; }
        public Visibility BackgroundVisibility { get => TitleBackground.Visibility; set => TitleBackground.Visibility = value; }

        public bool IsBackEnable { get => BackButton.IsEnabled; set => BackButton.IsEnabled = value; }
        public double TitleHeight { get => TitleGrid.Height; set => TitleGrid.Height = value; }
        public object RightAreaContent { get => UserContentPresenter.Content; set => UserContentPresenter.Content = value; }

        public TitleBar() => InitializeComponent();

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => RefreshEvent?.Invoke(sender, e);

        private void BackButton_Click(object sender, RoutedEventArgs e) => BackRequested?.Invoke(sender, e);

        private void TitleGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.OriginalSource is Grid || (e.OriginalSource is TextBlock a && a == TitleBlock))
            { RefreshEvent?.Invoke(sender, e); }
        }

        public void ShowProgressRing()
        {
            ProgressRing.IsActive = true;
            ProgressRing.Visibility = Visibility.Visible;
        }

        public void HideProgressRing()
        {
            ProgressRing.Visibility = Visibility.Collapsed;
            ProgressRing.IsActive = false;
        }
    }
}
