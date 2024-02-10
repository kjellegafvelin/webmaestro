using System;
using System.Globalization;
using System.Windows.Data;
using WebMaestro.ViewModels;

namespace WebMaestro.Converters
{
    internal class TabItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Controls.CloseableTabItem tabItem)
            {
                return tabItem.Header;
            }
            else if (value is TabItemViewModel vm)
            {
                return vm;
            }
            else
            {
                return null;
            }
        }
    }
}
