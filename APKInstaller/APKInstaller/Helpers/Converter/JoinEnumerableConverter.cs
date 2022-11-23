using Microsoft.UI.Xaml.Data;
using System;
using System.Collections;
using System.Linq;

namespace APKInstaller.Helpers.Converter
{
    public class JoinEnumerableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is IEnumerable list ? string.Join(parameter.ToString(), list.Cast<object>()) : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
