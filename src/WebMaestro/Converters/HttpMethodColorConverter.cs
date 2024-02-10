using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WebMaestro.ViewModels;

namespace WebMaestro.Converters
{
    public class HttpMethodColorConverter : IValueConverter
    {
        private static readonly SolidColorBrush GetBrush = new((Color)ColorConverter.ConvertFromString("#61affe"));
        private static readonly SolidColorBrush PostBrush = new((Color)ColorConverter.ConvertFromString("#49cc90"));
        private static readonly SolidColorBrush PutBrush = new((Color)ColorConverter.ConvertFromString("#fca130"));
        private static readonly SolidColorBrush DeleteBrush = new((Color)ColorConverter.ConvertFromString("#f93e3e"));
        private static readonly SolidColorBrush OptionsBrush = new((Color)ColorConverter.ConvertFromString("#0d5aa7"));
        private static readonly SolidColorBrush PatchBrush = new((Color)ColorConverter.ConvertFromString("#50e3c2"));
        private static readonly SolidColorBrush HeadBrush = new((Color)ColorConverter.ConvertFromString("#9012fe"));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var method = (HttpMethods)value;

            var color = method switch
            {
                HttpMethods.GET => GetBrush,
                HttpMethods.POST => PostBrush,
                HttpMethods.PATCH => PatchBrush,
                HttpMethods.DELETE => DeleteBrush,
                HttpMethods.PUT => PutBrush,
                HttpMethods.OPTIONS => OptionsBrush,
                HttpMethods.HEAD => HeadBrush,
                _ => null
            };

            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
