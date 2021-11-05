using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;

namespace APKInstaller.Helpers.ValueConverters
{
    public class TrueToFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => Convert(value, parameter);

        public object ConvertBack(object value, Type targetType, object parameter, string language) => Convert(value, parameter);

        private static object Convert(object value, object parameter)
        {
            if (parameter == null) { return !(bool)value; }
            else
            {
                switch (parameter)
                {
                    case "bool":
                        return !(bool)value;
                    case "ScrollMode":
                        return (ScrollMode)value == ScrollMode.Disabled ? ScrollMode.Auto : ScrollMode.Disabled;
                    case "Visibility":
                        return (Visibility)value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    default:
                        return value == null ? true : false;
                }
            }
        }
    }
}
