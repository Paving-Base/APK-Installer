using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
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
    public sealed partial class CapabilitiesInfoControl : UserControl
    {
        public CapabilitiesInfoControl() => InitializeComponent();

        public string HeadText
        {
            get => HeaderTextBlock.Text;
            set => HeaderTextBlock.Text = value ?? string.Empty;
        }

        private List<string> _capabilitiesList;
        public List<string> CapabilitiesList
        {
            get => _capabilitiesList;
            set
            {
                if (value != _capabilitiesList)
                {
                    _capabilitiesList = value;
                    GetTextBlock();
                }
            }
        }

        private void GetTextBlock()
        {
            if (CapabilitiesList == null) { return; }
            if (CapabilitiesList.Count > 3) { MoreButton.Visibility = Visibility.Visible; }
            foreach (string capability in CapabilitiesList)
            {
                if (!string.IsNullOrEmpty(capability))
                {
                    Paragraph paragraph = new Paragraph();
                    paragraph.Inlines.Add(new Run { Text = $"• {capability}" });
                    RichTextBlockFullCapabilities.Blocks.Add(paragraph);
                }
                if (RichTextBlockCapabilities.Blocks.Count < 3 && !string.IsNullOrEmpty(capability))
                {
                    Paragraph paragraph = new Paragraph();
                    paragraph.Inlines.Add(new Run { Text = $"• {capability}" });
                    RichTextBlockCapabilities.Blocks.Add(paragraph);
                }
            }
        }

        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            MoreButton.Visibility = Visibility.Collapsed;
            Root.BorderThickness = new Thickness(1, 0, 0, 0);
            RichTextBlockCapabilities.Visibility = Visibility.Collapsed;
            RichTextBlockFullCapabilities.Visibility = Visibility.Visible;
            _ = RichTextBlockFullCapabilities.Focus(FocusState.Pointer);
        }

        private void Root_LostFocus(object sender, RoutedEventArgs e)
        {
            MoreButton.Visibility = Visibility.Visible;
            Root.BorderThickness = new Thickness(0, 0, 0, 0);
            RichTextBlockCapabilities.Visibility = Visibility.Visible;
            RichTextBlockFullCapabilities.Visibility = Visibility.Collapsed;
        }
    }
}
