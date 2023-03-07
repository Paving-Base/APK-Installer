using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using QRCoder;
using System;
using System.ComponentModel;
using Windows.Foundation;

namespace APKInstaller.Controls
{
    public class QRCode : Control
    {
        #region Text

        /// <summary>
        /// Identifies the <see cref="Text"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(Setting),
                new PropertyMetadata(null, OnTextPropertyChanged));

        /// <summary>
        /// Gets or sets the Text.
        /// </summary>
        [Localizable(true)]
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((QRCode)d).OnTextChanged();
            }
        }

        #endregion

        /// <summary>
        /// Creates a new instance of the <see cref="QRCode"/> class.
        /// </summary>
        public QRCode()
        {
            DefaultStyleKey = typeof(QRCode);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            CreateQRCode();
        }

        private void OnTextChanged()
        {
            CreateQRCode();
        }

        private void CreateQRCode()
        {
            string payload = Text;

            using QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);

            using XamlQRCode qrCodeBmp = new XamlQRCode(qrCodeData);
            GeometryGroup qrCodeImageBmp = qrCodeBmp.GetGraphic(new Size(100, 100));

            if (GetTemplateChild("QRCodePath") is Path path)
            {
                path.Data = qrCodeImageBmp;
            }
        }
    }
}
