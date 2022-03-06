using APKInstaller.Models;
using CommunityToolkit.WinUI.Connectivity;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Controls
{
    public sealed partial class WebXAML : UserControl
    {
        public static readonly DependencyProperty ContentInfoProperty = DependencyProperty.Register(
           "ContentInfo",
           typeof(GitInfo),
           typeof(WebXAML),
           new PropertyMetadata(default(GitInfo), OnContentChanged));

        public GitInfo ContentInfo
        {
            get => (GitInfo)GetValue(ContentInfoProperty);
            set => SetValue(ContentInfoProperty, value);
        }

        public static readonly DependencyProperty ContentUrlProperty = DependencyProperty.Register(
           "ContentUrl",
           typeof(Uri),
           typeof(WebXAML),
           new PropertyMetadata(default(Uri), OnContentChanged));

        public Uri ContentUrl
        {
            get => (Uri)GetValue(ContentUrlProperty);
            set => SetValue(ContentUrlProperty, value);
        }

        public static readonly DependencyProperty ContentXAMLProperty = DependencyProperty.Register(
           "ContentXAML",
           typeof(string),
           typeof(WebXAML),
           new PropertyMetadata(default(string), OnContentChanged));

        public string ContentXAML
        {
            get => (string)GetValue(ContentXAMLProperty);
            set => SetValue(ContentXAMLProperty, value);
        }

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as WebXAML).UpdateContent(e.NewValue);

        public WebXAML() => InitializeComponent();

        private async void UpdateContent(object Content)
        {
            if (Content == null) { return; }
            if (Content is GitInfo ContentInfo && ContentInfo != default(GitInfo))
            {
                string value = ContentInfo.FormatURL(GitInfo.GITHUB_API);
                if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable) { return; }
                using (HttpClient client = new HttpClient())
                {
                    UIElement UIElement = null;
                    try
                    {
                        UIElement = (UIElement)XamlReader.Load(await client.GetStringAsync(value));
                    }
                    catch
                    {
                        try
                        {
                            UIElement = (UIElement)XamlReader.Load((await client.GetStringAsync(ContentInfo.FormatURL(GitInfo.FASTGIT_API))).Replace("://raw.githubusercontent.com", "://raw.fastgit.org"));
                        }
                        catch
                        {
                            try
                            {
                                UIElement = (UIElement)XamlReader.Load(await client.GetStringAsync(ContentInfo.FormatURL(GitInfo.JSDELIVR_API)));
                            }
                            catch
                            {
                                UIElement = null;
                            }
                        }
                    }
                    finally
                    {
                        if (UIElement != null)
                        {
                            this.Content = UIElement;
                        }
                    }
                }
            }
            else if (Content is string ContentXAML && ContentXAML != default)
            {
                UIElement UIElement = null;
                try
                {
                    UIElement = (UIElement)XamlReader.Load(ContentXAML);
                }
                catch
                {
                    UIElement = null;
                }
                finally
                {
                    if (UIElement != null)
                    {
                        this.Content = UIElement;
                    }
                }
            }
            else if (Content is Uri ContentUri && ContentUri != default)
            {
                if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable) { return; }
                using (HttpClient client = new HttpClient())
                {
                    UIElement UIElement = null;
                    try
                    {
                        UIElement = (UIElement)XamlReader.Load(await client.GetStringAsync(ContentUri));
                    }
                    catch
                    {
                        try
                        {
                            UIElement = (UIElement)XamlReader.Load((await client.GetStringAsync(ContentUri.ToString().Replace("://raw.githubusercontent.com", "://raw.fastgit.org"))).Replace("://raw.githubusercontent.com", "://raw.fastgit.org"));
                        }
                        catch
                        {
                            UIElement = null;
                        }
                    }
                    finally
                    {
                        if (UIElement != null)
                        {
                            this.Content = UIElement;
                        }
                    }
                }
            }
        }
    }
}
