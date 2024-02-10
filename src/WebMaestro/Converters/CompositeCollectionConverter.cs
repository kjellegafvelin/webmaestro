using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using WebMaestro.Models;

namespace WebMaestro.Converters
{
    /// <summary>
    /// A converter that organizes several collections into (optional)
    /// child collections that are put into <see cref="FolderItem"/>
    /// containers.
    /// </summary>
    public class CompositeCollectionConverter : IMultiValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var items = new CompositeCollection();

            foreach (object value in values)
            {
                if (value is IEnumerable<ObservableObject> collection)
                {
                    var container = new CollectionContainer
                    {
                        Collection = collection
                    };

                    items.Add(container);
                }
                else if (value is ObservableObject observable)
                {
                    items.Add(observable);
                }
            }

            return items;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot perform reverse-conversion");
        }
    }
}
