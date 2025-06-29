using APKInstaller.Helpers;
using APKInstaller.Models;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Connectivity;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using System;
using System.Net.Http;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Controls
{
    public sealed partial class WebXAML : UserControl
    {
        public static readonly DependencyProperty ContentInfoProperty =
            DependencyProperty.Register(
                nameof(ContentInfo),
                typeof(GitInfo),
                typeof(WebXAML),
                new PropertyMetadata(default(GitInfo), OnContentChanged));

        public GitInfo ContentInfo
        {
            get => (GitInfo)GetValue(ContentInfoProperty);
            set => SetValue(ContentInfoProperty, value);
        }

        public static readonly DependencyProperty ContentUrlProperty =
            DependencyProperty.Register(
                nameof(ContentUrl),
                typeof(Uri),
                typeof(WebXAML),
                new PropertyMetadata(default(Uri), OnContentChanged));

        public Uri ContentUrl
        {
            get => (Uri)GetValue(ContentUrlProperty);
            set => SetValue(ContentUrlProperty, value);
        }

        public static readonly DependencyProperty ContentXAMLProperty =
            DependencyProperty.Register(
                nameof(ContentXAML),
                typeof(string),
                typeof(WebXAML),
                new PropertyMetadata(default(string), OnContentChanged));

        public string ContentXAML
        {
            get => (string)GetValue(ContentXAMLProperty);
            set => SetValue(ContentXAMLProperty, value);
        }

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as WebXAML).UpdateContent(e.NewValue);
        }

        public WebXAML() => InitializeComponent();

        private async void UpdateContent(object Content)
        {
            await Task.Run(async () =>
            {
                if (Content == null) { return; }
                if (Content is GitInfo ContentInfo && ContentInfo != default(GitInfo))
                {
                    string value = ContentInfo.FormatURL(GitInfo.GITHUB_API);
                    if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable) { return; }
                    using HttpClient client = new();
                    UIElement UIElement = null;
                    try
                    {
                        string xaml = await client.GetStringAsync(value);
                        if (string.IsNullOrWhiteSpace(xaml)) { throw new ArgumentNullException(nameof(Content), $"The text fetched from {value} is empty."); }
                        UIElement = await DispatcherQueue.EnqueueAsync(() => { return (UIElement)XamlReader.Load(xaml); });
                    }
                    catch (Exception e)
                    {
                        SettingsHelper.LogManager.GetLogger(nameof(WebXAML)).Warn(e.ExceptionToMessage(), e);
                        try
                        {
                            string xaml = await client.GetStringAsync(ContentInfo.FormatURL(GitInfo.JSDELIVR_API));
                            if (string.IsNullOrWhiteSpace(xaml)) { throw new ArgumentNullException(nameof(Content), $"The text fetched from {ContentInfo.FormatURL(GitInfo.JSDELIVR_API)} is empty."); }
                            UIElement = await DispatcherQueue.EnqueueAsync(() => { return (UIElement)XamlReader.Load(xaml); });
                        }
                        catch (Exception ex)
                        {
                            SettingsHelper.LogManager.GetLogger(nameof(WebXAML)).Warn(ex.ExceptionToMessage(), ex);
                            UIElement = null;
                        }
                    }
                    finally
                    {
                        if (UIElement != null)
                        {
                            _ = DispatcherQueue.EnqueueAsync(() => this.Content = UIElement);
                        }
                    }
                }
                else if (Content is string ContentXAML && ContentXAML != default)
                {
                    UIElement UIElement = null;
                    try
                    {
                        UIElement = await DispatcherQueue.EnqueueAsync(() => { return (UIElement)XamlReader.Load(ContentXAML); });
                    }
                    catch (Exception e)
                    {
                        SettingsHelper.LogManager.GetLogger(nameof(WebXAML)).Warn(e.ExceptionToMessage(), e);
                        UIElement = null;
                    }
                    finally
                    {
                        if (UIElement != null)
                        {
                            _ = DispatcherQueue.EnqueueAsync(() => this.Content = UIElement);
                        }
                    }
                }
                else if (Content is Uri ContentUri && ContentUri != default)
                {
                    if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable) { return; }
                    using HttpClient client = new();
                    UIElement UIElement = null;
                    try
                    {
                        string xaml = await client.GetStringAsync(ContentUri);
                        if (string.IsNullOrWhiteSpace(xaml)) { throw new ArgumentNullException(nameof(Content), $"The text fetched from {ContentUri} is empty."); }
                        UIElement = await DispatcherQueue.EnqueueAsync(() => { return (UIElement)XamlReader.Load(xaml); });
                    }
                    catch (Exception e)
                    {
                        SettingsHelper.LogManager.GetLogger(nameof(WebXAML)).Warn(e.ExceptionToMessage(), e);
                        UIElement = null;
                    }
                    finally
                    {
                        if (UIElement != null)
                        {
                            _ = DispatcherQueue.EnqueueAsync(() => this.Content = UIElement);
                        }
                    }
                }
            });
        }
    }
}
