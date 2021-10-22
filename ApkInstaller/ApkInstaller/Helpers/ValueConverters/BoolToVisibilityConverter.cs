using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace APKInstaller.Helpers.ValueConverters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch ((string)parameter)
            {
                case "bool": return (bool)value ? Visibility.Visible : Visibility.Collapsed;
                case "!bool": return (bool)value ? Visibility.Collapsed : Visibility.Visible;
                case "string": return !string.IsNullOrEmpty((string)value) ? Visibility.Visible : Visibility.Collapsed;
                default: return value is bool boolean ? boolean ? Visibility.Visible : Visibility.Collapsed : value != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => (Visibility)value == Visibility.Visible;
    }
}
