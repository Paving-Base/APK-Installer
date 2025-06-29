using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;
using System;
using System.Collections;
using System.Linq;

namespace APKInstaller.Helpers.Converter
{
    public partial class JoinEnumerableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            object result = value is IEnumerable list ? string.Join(parameter.ToString(), list.Cast<object>()) : value;
            return targetType.IsInstanceOfType(result) ? result : XamlBindingHelper.ConvertValue(targetType, result);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return targetType.IsInstanceOfType(value) ? value : XamlBindingHelper.ConvertValue(targetType, value);
        }
    }
}
