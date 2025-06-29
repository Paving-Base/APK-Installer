using APKInstaller.Helpers;
using APKInstaller.Models;
using CommunityToolkit.WinUI.Connectivity;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Controls.Dialogs
{
    public sealed partial class MarkdownDialog : ContentDialog, INotifyPropertyChanged
    {
        private object title;

        public static readonly DependencyProperty FallbackContentProperty = DependencyProperty.Register(
           "FallbackContent",
           typeof(string),
           typeof(MarkdownDialog),
           new PropertyMetadata("{0}"));

        public string FallbackContent
        {
            get => (string)GetValue(FallbackContentProperty);
            set => SetValue(FallbackContentProperty, value);
        }

        public static readonly DependencyProperty ContentInfoProperty = DependencyProperty.Register(
           "ContentInfo",
           typeof(GitInfo),
           typeof(MarkdownDialog),
           new PropertyMetadata(default(GitInfo), OnContentChanged));

        public GitInfo ContentInfo
        {
            get => (GitInfo)GetValue(ContentInfoProperty);
            set => SetValue(ContentInfoProperty, value);
        }

        public static readonly DependencyProperty ContentUrlProperty = DependencyProperty.Register(
           "ContentUrl",
           typeof(Uri),
           typeof(MarkdownDialog),
           new PropertyMetadata(default(Uri), OnContentChanged));

        public Uri ContentUrl
        {
            get => (Uri)GetValue(ContentUrlProperty);
            set => SetValue(ContentUrlProperty, value);
        }

        public static readonly DependencyProperty ContentTextProperty = DependencyProperty.Register(
           "ContentText",
           typeof(string),
           typeof(MarkdownDialog),
           new PropertyMetadata(default(string), OnContentChanged));

        public string ContentText
        {
            get => (string)GetValue(ContentTextProperty);
            set => SetValue(ContentTextProperty, value);
        }

        public static readonly DependencyProperty ContentTaskProperty = DependencyProperty.Register(
           "ContentTask",
           typeof(Func<Task<string>>),
           typeof(MarkdownDialog),
           new PropertyMetadata(null, OnContentChanged));

        public Func<Task<string>> ContentTask
        {
            get => (Func<Task<string>>)GetValue(ContentTaskProperty);
            set => SetValue(ContentTaskProperty, value);
        }

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MarkdownDialog).UpdateContent(e.NewValue);
        }

        private bool isInitialized;
        internal bool IsInitialized
        {
            get => isInitialized;
            private set
            {
                isInitialized = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public MarkdownDialog() => InitializeComponent();

        private async void UpdateContent(object Content)
        {
            IsInitialized = false;
            title = Title ?? title;
            if (Content == null) { return; }
            if (Content is GitInfo ContentInfo && ContentInfo != default(GitInfo))
            {
                string value = ContentInfo.FormatURL(GitInfo.GITHUB_API);
                if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                {
                    MarkdownText.Text = string.Format(FallbackContent, value);
                    Title ??= title;
                    return;
                }
                using HttpClient client = new();
                try
                {
                    string text = await client.GetStringAsync(value);
                    if (string.IsNullOrWhiteSpace(text)) { throw new ArgumentNullException(nameof(Content), $"The text fetched from {value} is empty."); }
                    MarkdownText.Text = text;
                    Title = null;
                }
                catch (Exception e)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(MarkdownDialog)).Warn(e.ExceptionToMessage(), e);
                    try
                    {
                        string text = await client.GetStringAsync(ContentInfo.FormatURL(GitInfo.JSDELIVR_API));
                        if (string.IsNullOrWhiteSpace(text)) { throw new ArgumentNullException(nameof(Content), $"The text fetched from the {ContentInfo.FormatURL(GitInfo.JSDELIVR_API)} is empty."); }
                        MarkdownText.Text = text;
                        Title = null;
                    }
                    catch (Exception ex)
                    {
                        SettingsHelper.LogManager.GetLogger(nameof(MarkdownDialog)).Warn(ex.ExceptionToMessage(), ex);
                        MarkdownText.Text = string.Format(FallbackContent, value);
                        Title ??= title;
                    }
                }
            }
            else if (Content is Func<Task<string>> ContentTask && ContentTask != default)
            {
                try
                {
                    string text = await ContentTask();
                    if (string.IsNullOrWhiteSpace(text)) { throw new ArgumentNullException(nameof(Content), "The text fetched from Task is empty."); }
                    MarkdownText.Text = text;
                    Title = null;
                }
                catch (Exception e)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(MarkdownDialog)).Warn(e.ExceptionToMessage(), e);
                    MarkdownText.Text = FallbackContent;
                    Title ??= title;
                }
            }
            else if (Content is string ContentText && ContentText != default)
            {
                MarkdownText.Text = ContentText;
                Title = null;
            }
            else if (Content is Uri ContentUri && ContentUri != default)
            {
                if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                {
                    MarkdownText.Text = string.Format(FallbackContent, ContentUri.ToString());
                    Title ??= title;
                    return;
                }
                using HttpClient client = new();
                try
                {
                    string text = await client.GetStringAsync(ContentUri);
                    if (string.IsNullOrWhiteSpace(text)) { throw new ArgumentNullException(nameof(Content), $"The text fetched from {ContentUri} is empty."); }
                    MarkdownText.Text = text;
                    Title = null;
                }
                catch (Exception e)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(MarkdownDialog)).Warn(e.ExceptionToMessage(), e);
                    MarkdownText.Text = string.Format(FallbackContent, ContentUri.ToString());
                    Title ??= title;
                }
            }
            IsInitialized = true;
        }

        private void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e) => _ = Launcher.LaunchUriAsync(new Uri(e.Link));
    }
}
