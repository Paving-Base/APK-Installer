using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace AAPTForNet.Models
{
    public class Icon
    {

        private const int hdpiWidth = 72;
        public const string DefaultName = "ic_launcher.png";

        internal static readonly Icon Default = new Icon(DefaultName);

        /// <summary>
        /// Return absolute path to package icon if @isImage is true,
        /// otherwise return empty string
        /// </summary>
        public string RealPath { get; set; }

        /// <summary>
        /// Determines whether icon of package is an image
        /// </summary>
        public bool isImage => !DefaultName.Equals(IconName) && !isMarkup;

        internal bool isMarkup => IconName
            .EndsWith(".xml", StringComparison.OrdinalIgnoreCase);

        // Not real icon, it refer to another
        internal bool isRefernce => IconName.StartsWith("0x");

        internal bool isHighDensity
        {
            get
            {
                if (!isImage || !File.Exists(RealPath))
                {
                    return false;
                }

                try
                {
                    // Load from unsupported format will throw an exception.
                    // But icon can be packed without extension
                    using (Bitmap image = new Bitmap(RealPath))
                    {
                        return image.Width > hdpiWidth;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Icon name can be an asset image (real icon image),
        /// markup file (actually it's image, but packed to xml)
        /// or reference to another
        /// </summary>
        internal string IconName { get; set; }

        internal Icon() => throw new NotImplementedException();

        internal Icon(string iconName)
        {
            IconName = iconName ?? string.Empty;
            RealPath = $@"{AppDomain.CurrentDomain.BaseDirectory}\256x256.png";
        }

        public override string ToString() => IconName;

        public override bool Equals(object obj)
        {
            if (obj is Icon ic)
            {
                return IconName == ic.IconName;
            }
            return false;
        }

        public override int GetHashCode() => -489061483 + EqualityComparer<string>.Default.GetHashCode(IconName);
    }
}
