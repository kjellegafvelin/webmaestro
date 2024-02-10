using System;
using System.Globalization;
using System.Windows.Data;

namespace WebMaestro.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : ValueConverterMarkupExtension<InverseBooleanConverter>
    {
        public override object Convert(object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (targetType != typeof(bool?) && targetType != typeof(bool))
            {
                throw new InvalidOperationException("The target must be a boolean");
            }

            return !(bool)value;
        }

        public override object ConvertBack(object value, Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (targetType != typeof(bool?) && targetType != typeof(bool))
            {
                throw new InvalidOperationException("The target must be a boolean");
            }

            return !(bool)value;
        }
    }
}
