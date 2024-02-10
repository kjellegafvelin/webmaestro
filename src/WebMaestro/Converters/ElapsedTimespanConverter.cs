using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WebMaestro.Converters
{
    class ElapsedTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var elapsed = (TimeSpan)value;

            if (elapsed.Minutes > 0)
            {
                return string.Format($"{elapsed.Minutes}m {elapsed.Seconds}s");
            }
            else if (elapsed.Seconds > 0)
            {
                return string.Format($"{elapsed.Seconds}s {elapsed.Milliseconds}ms");
            }
            else
            {
                return string.Format($"{elapsed.Milliseconds}ms");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
