using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using QRCoder;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;

namespace APKInstaller.Controls
{
    [ContentProperty(Name = "Content")]
    public partial class QRCode : Control
    {
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

        private void OnContentChanged()
        {
            CreateQRCode();
        }

        private void CreateQRCode()
        {
            QRCodeTemplateSettings templateSettings = TemplateSettings;

            if (Content == null)
            {
                templateSettings.GeometryGroup = null;
                return;
            }

            QRCodeData qrCodeData;

            if (Content is IEnumerable<byte> array)
            {
                byte[] payload = array.ToArray();
                using QRCodeGenerator qrGenerator = new();
                qrCodeData = qrGenerator.CreateQrCode(payload, ECCLevel);
            }
            else
            {
                string payload = Content.ToString();
                using QRCodeGenerator qrGenerator = new();
                qrCodeData = qrGenerator.CreateQrCode(payload, ECCLevel, IsForceUTF8, IsUTF8BOM, EciMode, RequestedVersion);
            }

            using XamlQRCode qrCodeBmp = new(qrCodeData);
            GeometryGroup qrCodeImageBmp = qrCodeBmp.GetGraphic(new Size(100, 100));

            templateSettings.GeometryGroup = qrCodeImageBmp;
        }
    }
}
