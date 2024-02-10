using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WebMaestro.Converters
{
    class BooleanToInverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool)
            {
                flag = (bool)value;
            }
            else if (value is bool?)
            {
                bool? nullable = (bool?)value;
                flag = nullable.Value;
            }
            return (flag ? Visibility.Collapsed : Visibility.Visible);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((value is Visibility) && (((Visibility)value) == Visibility.Collapsed));

        }
    }
}
