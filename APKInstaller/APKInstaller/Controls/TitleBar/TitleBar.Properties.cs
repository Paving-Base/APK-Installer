using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace APKInstaller.Controls
{
    public partial class TitleBar
    {
        public static readonly DependencyProperty CustomContentProperty =
            DependencyProperty.Register(
                "CustomContent",
                typeof(object),
                typeof(TitleBar),
                new PropertyMetadata(default, OnCustomContentPropertyChanged));

        public static readonly DependencyProperty AutoSuggestBoxProperty =
            DependencyProperty.Register(
                "AutoSuggestBox",
                typeof(AutoSuggestBox),
                typeof(TitleBar),
                new PropertyMetadata(default, OnCustomContentPropertyChanged));

        public static readonly DependencyProperty PaneFooterProperty =
            DependencyProperty.Register(
                "PaneFooter",
                typeof(object),
                typeof(TitleBar),
                new PropertyMetadata(default, OnCustomContentPropertyChanged));

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(
                "IconSource",
                typeof(UIElement),
                typeof(TitleBar),
                new PropertyMetadata(default(UIElement), OnIconSourcePropertyChanged));

        public static readonly DependencyProperty IsBackButtonVisibleProperty =
            DependencyProperty.Register(
                "IsBackButtonVisible",
                typeof(bool),
                typeof(TitleBar),
                new PropertyMetadata(default(bool), OnIsBackButtonVisiblePropertyChanged));

        public static readonly DependencyProperty IsBackEnabledProperty =
            DependencyProperty.Register(
                "IsBackEnabled",
                typeof(bool),
                typeof(TitleBar),
                new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsRefreshButtonVisibleProperty =
            DependencyProperty.Register(
                "IsRefreshButtonVisible",
                typeof(bool),
                typeof(TitleBar),
                new PropertyMetadata(default(bool), OnIsRefreshButtonVisiblePropertyChanged));

        public static readonly DependencyProperty IsRefreshEnabledProperty =
            DependencyProperty.Register(
                "IsRefreshEnabled",
                typeof(bool),
                typeof(TitleBar),
                new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty TemplateSettingsProperty =
            DependencyProperty.Register(
                "TemplateSettings",
                typeof(TitleBarTemplateSettings),
                typeof(TitleBar),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                "Title",
                typeof(string),
                typeof(TitleBar),
                new PropertyMetadata(default(string), OnTitlePropertyChanged));

        public object CustomContent
        {
            get => GetValue(CustomContentProperty);
            set => SetValue(CustomContentProperty, value);
        }

        public object AutoSuggestBox
        {
            get => (AutoSuggestBox)GetValue(AutoSuggestBoxProperty);
            set => SetValue(AutoSuggestBoxProperty, value);
        }

        public object PaneFooter
        {
            get => GetValue(PaneFooterProperty);
            set => SetValue(PaneFooterProperty, value);
        }

        public UIElement IconSource
        {
            get => (UIElement)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public bool IsBackButtonVisible
        {
            get => (bool)GetValue(IsBackButtonVisibleProperty);
            set => SetValue(IsBackButtonVisibleProperty, value);
        }

        public bool IsBackEnabled
        {
            get => (bool)GetValue(IsBackEnabledProperty);
            set => SetValue(IsBackEnabledProperty, value);
        }

        public bool IsRefreshButtonVisible
        {
            get => (bool)GetValue(IsRefreshButtonVisibleProperty);
            set => SetValue(IsRefreshButtonVisibleProperty, value);
        }

        public bool IsRefreshEnabled
        {
            get => (bool)GetValue(IsRefreshEnabledProperty);
            set => SetValue(IsRefreshEnabledProperty, value);
        }

        public TitleBarTemplateSettings TemplateSettings
        {
            get => (TitleBarTemplateSettings)GetValue(TemplateSettingsProperty);
            set => SetValue(TemplateSettingsProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public event TypedEventHandler<TitleBar, object> BackRequested;
        public event TypedEventHandler<TitleBar, object> RefreshRequested;

        private static void OnCustomContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TitleBar)d).OnCustomContentPropertyChanged(e);
        }

        private static void OnIconSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TitleBar)d).OnIconSourcePropertyChanged(e);
        }

        private static void OnIsBackButtonVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TitleBar)d).OnIsBackButtonVisiblePropertyChanged(e);
        }

        private static void OnIsRefreshButtonVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TitleBar)d).OnIsRefreshButtonVisiblePropertyChanged(e);
        }

        private static void OnTitlePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TitleBar)d).OnTitlePropertyChanged(e);
        }
    }
}
