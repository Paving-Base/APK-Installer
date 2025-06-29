using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;

namespace APKInstaller.Controls
{
    [ContentProperty(Name = "Content")]
    [TemplatePart(Name = QRCodePathName, Type = typeof(Path))]
    public partial class QRCode : Control
    {
        private const string QRCodePathName = "PART_QRCodePath";

        private Path QRCodePath;
        private QRCodeData QRCodeData;

        /// <summary>
        /// Creates a new instance of the <see cref="QRCode"/> class.
        /// </summary>
        public QRCode()
        {
            DefaultStyleKey = typeof(QRCode);
            SetValue(TemplateSettingsProperty, new QRCodeTemplateSettings());
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (QRCodePath != null)
            {
                QRCodePath.SizeChanged -= OnSizeChanged;
            }

            QRCodePath = GetTemplateChild(QRCodePathName) as Path;

            if (QRCodePath != null)
            {
                QRCodePath.SizeChanged += OnSizeChanged;
            }

            UpdateQRCodeData();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateQRCode();
        }

        private void OnContentChanged()
        {
            UpdateQRCodeData();
        }

        private void UpdateQRCodeData()
        {
            QRCodeData?.Dispose();

            if (Content == null)
            {
                QRCodeData = null;
                return;
            }

            if (Content is IEnumerable<byte> array)
            {
                byte[] payload = [.. array];
                using QRCodeGenerator qrGenerator = new();
                QRCodeData = qrGenerator.CreateQrCode(payload, ECCLevel);
            }
            else
            {
                string payload = Content.ToString();
                using QRCodeGenerator qrGenerator = new();
                QRCodeData = qrGenerator.CreateQrCode(payload, ECCLevel, IsForceUTF8, IsUTF8BOM, EciMode, RequestedVersion);
            }

            UpdateQRCode();
        }

        private void UpdateQRCode()
        {
            QRCodeTemplateSettings templateSettings = TemplateSettings;

            if (QRCodeData == null)
            {
                templateSettings.GeometryGroup = null;
                return;
            }

            Size size = new(100, 100);
            if (QRCodePath != null)
            {
                double length = Math.Min(QRCodePath.ActualWidth, QRCodePath.ActualHeight);
                size = new Size(length, length);
            }

            using XamlQRCode qrCodeBmp = new(QRCodeData, false);
            GeometryGroup qrCodeImageBmp = qrCodeBmp.GetGraphic(size);

            templateSettings.GeometryGroup = qrCodeImageBmp;
        }
    }
}
