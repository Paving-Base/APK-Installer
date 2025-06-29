namespace APKInstaller.Helpers.Converter
{
    /// <summary>
    /// This class converts a string value into a Boolean value (if the value is matched returns a true value).
    /// </summary>
    public partial class StringRegexToBoolConverter : StringRegexToObjectConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringRegexToBoolConverter"/> class.
        /// </summary>
        public StringRegexToBoolConverter()
        {
            MatchValue = true;
            NotMatchValue = false;
        }
    }
}
