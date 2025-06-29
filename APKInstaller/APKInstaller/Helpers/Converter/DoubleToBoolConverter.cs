using CommunityToolkit.WinUI.UI.Converters;

namespace APKInstaller.Helpers.Converter
{
    /// <summary>
    /// This class converts a double value into a Boolean value (if the value is null or empty returns a false value).
    /// </summary>
    public partial class DoubleToBoolConverter : DoubleToObjectConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleToBoolConverter"/> class.
        /// </summary>
        public DoubleToBoolConverter()
        {
            TrueValue = true;
            FalseValue = false;
            NullValue = false;
            GreaterThan = 0;
        }
    }
}
