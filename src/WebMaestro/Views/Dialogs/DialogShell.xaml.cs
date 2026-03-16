using System.Windows;
using System.Windows.Controls;

namespace WebMaestro.Views.Dialogs
{
    public enum DialogButtons
    {
        Ok,
        OkCancel,
        Cancel
    }


    public partial class DialogShell : UserControl
    {
        public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register(
            nameof(HeaderText),
            typeof(string),
            typeof(DialogShell),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ButtonsProperty = DependencyProperty.Register(
            nameof(Buttons),
            typeof(DialogButtons),
            typeof(DialogShell),
            new PropertyMetadata(DialogButtons.Ok));

        public DialogShell()
        {
            InitializeComponent();
        }

        public string HeaderText
        {
            get => (string)GetValue(HeaderTextProperty);
            set => SetValue(HeaderTextProperty, value);
        }

        public DialogButtons Buttons
        {
            get => (DialogButtons)GetValue(ButtonsProperty);
            set => SetValue(ButtonsProperty, value);
        }

    }
}
