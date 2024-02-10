using System.Windows;
using System.Windows.Controls;
using WebMaestro.ViewModels;

namespace WebMaestro.Selectors
{
    internal class TabItemStyleSelector : StyleSelector
    {
        public Style RequestStyle { get; set; }

        public Style ServerStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            return item is WebViewModel ? this.RequestStyle : this.ServerStyle;
        }
    }
}
