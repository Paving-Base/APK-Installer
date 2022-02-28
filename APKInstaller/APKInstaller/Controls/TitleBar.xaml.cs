using APKInstaller.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Controls
{
    public sealed partial class TitleBar : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
           "Title",
           typeof(string),
           typeof(TitleBar),
           new PropertyMetadata(default(string), null));

        public static readonly DependencyProperty TitleHeightProperty = DependencyProperty.Register(
           "TitleHeight",
           typeof(double),
           typeof(TitleBar),
           new PropertyMetadata(UIHelper.PageTitleHeight, null));

        public static readonly DependencyProperty IsBackEnableProperty = DependencyProperty.Register(
           "IsBackEnable",
           typeof(bool),
           typeof(TitleBar),
           new PropertyMetadata(true, null));

        public static readonly DependencyProperty RightAreaContentProperty = DependencyProperty.Register(
           "RightAreaContent",
           typeof(object),
           typeof(TitleBar),
           null);

        public static readonly DependencyProperty BackgroundVisibilityProperty = DependencyProperty.Register(
           "BackgroundVisibility",
           typeof(Visibility),
           typeof(TitleBar),
           new PropertyMetadata(Visibility.Collapsed, null));

        public static readonly DependencyProperty BackButtonVisibilityProperty = DependencyProperty.Register(
           "BackButtonVisibility",
           typeof(Visibility),
           typeof(TitleBar),
           new PropertyMetadata(Visibility.Visible, null));

        public static readonly DependencyProperty RefreshButtonVisibilityProperty = DependencyProperty.Register(
           "RefreshButtonVisibility",
           typeof(Visibility),
           typeof(TitleBar),
           new PropertyMetadata(Visibility.Collapsed, null));

        [Localizable(true)]
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public double TitleHeight
        {
            get => (double)GetValue(TitleHeightProperty);
            set => SetValue(TitleHeightProperty, value);
        }

        public bool IsBackEnable
        {
            get => (bool)GetValue(IsBackEnableProperty);
            set => SetValue(IsBackEnableProperty, value);
        }

        public object RightAreaContent
        {
            get => GetValue(RightAreaContentProperty);
            set => SetValue(RightAreaContentProperty, value);
        }

        public Visibility BackgroundVisibility
        {
            get => (Visibility)GetValue(BackgroundVisibilityProperty);
            set => SetValue(BackgroundVisibilityProperty, value);
        }

        public Visibility BackButtonVisibility
        {
            get => (Visibility)GetValue(BackButtonVisibilityProperty);
            set => SetValue(BackButtonVisibilityProperty, value);
        }

        public Visibility RefreshButtonVisibility
        {
            get => (Visibility)GetValue(RefreshButtonVisibilityProperty);
            set => SetValue(RefreshButtonVisibilityProperty, value);
        }

        public event RoutedEventHandler RefreshEvent;
        public event RoutedEventHandler BackRequested;

        public TitleBar() => InitializeComponent();

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => RefreshEvent?.Invoke(sender, e);

        private void BackButton_Click(object sender, RoutedEventArgs e) => BackRequested?.Invoke(sender, e);

        private void TitleGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.OriginalSource is Grid || (e.OriginalSource is TextBlock a && a == TitleBlock))
            { RefreshEvent?.Invoke(sender, e); }
        }

        public void SetProgressValue(double value)
        {
            ProgressRing.Value = value;
            if (ProgressRing.IsIndeterminate)
            {
                ProgressRing.IsIndeterminate = false;
            }
        }

        public void ShowProgressRing()
        {
            ProgressRing.IsActive = true;
            if (!ProgressRing.IsIndeterminate)
            {
                ProgressRing.IsIndeterminate = true;
            }
            ProgressRing.Visibility = Visibility.Visible;
        }

        public void HideProgressRing()
        {
            ProgressRing.Visibility = Visibility.Collapsed;
            ProgressRing.IsActive = false;
        }
    }
}
