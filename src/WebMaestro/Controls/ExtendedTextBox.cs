using System.Windows;
using System.Windows.Controls;

namespace WebMaestro.Controls
{
    internal class ExtendedTextBox : TextBox
    {
        public static readonly DependencyProperty PlaceHolderProperty = DependencyProperty.Register("PlaceHolder", 
                                                                        typeof(object), 
                                                                        typeof(ExtendedTextBox), 
                                                                        new UIPropertyMetadata(null));
        public object PlaceHolder 
        { 
            get => (object)GetValue(PlaceHolderProperty); 
            set => SetValue(PlaceHolderProperty, value); 
        }

        public static readonly DependencyProperty PlaceHolderTemplateProperty = DependencyProperty.Register("PlaceHolderTemplate",
                                                                                typeof(DataTemplate),
                                                                                typeof(ExtendedTextBox),
                                                                                new UIPropertyMetadata(null));
        public DataTemplate PlaceHolderTemplate
        {
            get => (DataTemplate)GetValue(PlaceHolderTemplateProperty);
            set => SetValue(PlaceHolderTemplateProperty, value);
        }

        static ExtendedTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExtendedTextBox), new FrameworkPropertyMetadata(typeof(ExtendedTextBox)));
        }

        public ExtendedTextBox()
        {

        }
    }
}
