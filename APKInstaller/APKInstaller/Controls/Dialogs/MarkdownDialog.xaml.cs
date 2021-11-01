using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Controls.Dialogs
{
    public sealed partial class MarkdownDialog : ContentDialog, INotifyPropertyChanged
    {
        private bool isInitialized;
        internal bool IsInitialized
        {
            get => isInitialized;
            set
            {
                isInitialized = value;
                RaisePropertyChangedEvent();
            }
        }

        public string ContentUrl
        {
            set
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    if (string.IsNullOrEmpty(value)) { return; }
                    IsInitialized = false;
                    value = value.StartsWith("http") ? value : $"https://{value}";
                    using var client = new HttpClient();
                    try
                    {
                        MarkdownText.Text = await client.GetStringAsync(value);
                        Title = string.Empty;
                    }
                    catch
                    {
                        if (value.Contains("raw.githubusercontent.com"))
                        {
                            try
                            {
                                MarkdownText.Text = (await client.GetStringAsync(value.Replace("raw.githubusercontent.com", "raw.fastgit.org"))).Replace("raw.githubusercontent.com", "raw.fastgit.org");
                                Title = string.Empty;
                            }
                            catch
                            {
                                MarkdownText.Text = value;
                            }
                        }
                        else
                        {
                            MarkdownText.Text = value;
                        }
                    }
                    IsInitialized = true;
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public MarkdownDialog() => InitializeComponent();

        private void MarkdownText_LinkClicked(object sender, CommunityToolkit.WinUI.UI.Controls.LinkClickedEventArgs e)
        {
            _ = Launcher.LaunchUriAsync(new Uri(e.Link));
        }
    }
}
