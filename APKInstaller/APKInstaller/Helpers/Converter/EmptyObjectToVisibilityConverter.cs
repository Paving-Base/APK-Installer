using CommunityToolkit.WinUI.UI.Converters;
using Microsoft.UI.Xaml;

namespace APKInstaller.Helpers.Converter
{
    /// <summary>
    /// This class converts a object value into a Visibility value (if the value is null or empty returns a collapsed value).
    /// </summary>
    public partial class EmptyObjectToVisibilityConverter : EmptyObjectToObjectConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyObjectToVisibilityConverter"/> class.
        /// </summary>
        public EmptyObjectToVisibilityConverter()
        {
            NotEmptyValue = Visibility.Visible;
            EmptyValue = Visibility.Collapsed;
        }
    }
}
