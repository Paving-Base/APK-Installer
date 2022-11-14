using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Versioning;

namespace AAPT2ForNet.Models
{
    public class Icon
    {
        private const int hdpiWidth = 72;
        public const string DefaultName = "ic_launcher.png";

        internal static readonly Icon Default = new(DefaultName);

        /// <summary>
        /// Return absolute path to package icon if @isImage is true,
        /// otherwise return empty string
        /// </summary>
        public string RealPath { get; set; }

        /// <summary>
        /// Determines whether icon of package is an image
        /// </summary>
        public bool IsImage => !DefaultName.Equals(IconName, StringComparison.Ordinal) && !IsMarkup;

        internal bool IsMarkup => IconName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);

        // Not real icon, it refer to another
        internal bool IsRefernce => IconName.StartsWith("0x");

        [SupportedOSPlatform("windows")]
        internal bool IsHighDensity
        {
            get
            {
                if (!IsImage || !File.Exists(RealPath))
                {
                    return false;
                }

                try
                {
                    // Load from unsupported format will throw an exception.
                    // But icon can be packed without extension
                    using Bitmap image = new(RealPath);
                    return image.Width > hdpiWidth;
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

        internal Icon(string iconName)
        {
            IconName = iconName ?? string.Empty;
            RealPath = "/Assets/256x256.png";
        }

        public override string ToString() => IconName;

        public override bool Equals(object obj) => obj is Icon ic && IconName == ic.IconName;

        public override int GetHashCode() => -489061483 + EqualityComparer<string>.Default.GetHashCode(IconName);
    }
}
