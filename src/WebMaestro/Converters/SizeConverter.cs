using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WebMaestro.Converters
{
    class SizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var size = (long)value;

            if (size > 1024 * 1024)
            {
                var temp = Math.Round((double)size / (1024 * 1024), 2);
                return $"{temp} MB";
            }
            else if (size > 1024)
            {
                var temp = Math.Round((double)size / 1024, 2);
                return $"{temp} KB";
            }
            else if (size > -1)
            {
                return $"{size} B";
            }
            else 
            {
                return "N/A";
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
