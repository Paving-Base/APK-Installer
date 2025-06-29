using CommunityToolkit.WinUI.UI.Converters;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;
using System;
using System.Text.RegularExpressions;

namespace APKInstaller.Helpers.Converter
{
    public partial class StringRegexToObjectConverter : DependencyObject, IValueConverter
    {
        /// <summary>
        /// Identifies the <see cref="MatchValue"/> property.
        /// </summary>
        public static readonly DependencyProperty MatchValueProperty =
            DependencyProperty.Register(nameof(MatchValue), typeof(object), typeof(BoolToObjectConverter), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="NotMatchValue"/> property.
        /// </summary>
        public static readonly DependencyProperty NotMatchValueProperty =
            DependencyProperty.Register(nameof(NotMatchValue), typeof(object), typeof(BoolToObjectConverter), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the value to be returned when the string is matched
        /// </summary>
        public object MatchValue
        {
            get { return GetValue(MatchValueProperty); }
            set { SetValue(MatchValueProperty, value); }
        }

        /// <summary>
        /// Gets or sets the value to be returned when the string is not matched
        /// </summary>
        public object NotMatchValue
        {
            get { return GetValue(NotMatchValueProperty); }
            set { SetValue(NotMatchValueProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string text = parameter.ToString();
            bool isMatch = !string.IsNullOrWhiteSpace(text) && Regex.IsMatch(value.ToString(), text);
            object result = isMatch ? MatchValue : NotMatchValue;
            return targetType.IsInstanceOfType(result) ? result : XamlBindingHelper.ConvertValue(targetType, result);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return targetType.IsInstanceOfType(value) ? value : XamlBindingHelper.ConvertValue(targetType, value);
        }
    }
}
