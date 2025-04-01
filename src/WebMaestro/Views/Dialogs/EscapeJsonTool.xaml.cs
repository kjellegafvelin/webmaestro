using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows;
using WebMaestro.ViewModels.Dialogs;

namespace WebMaestro.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for EscapeJsonTool.xaml
    /// </summary>
    public partial class EscapeJsonTool : Window
    {
        private EscapeJsonToolViewModel vm;

        public EscapeJsonTool()
        {
            InitializeComponent();
            // Set JSON syntax highlighting
            SetHighlighting();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.vm = (EscapeJsonToolViewModel)this.DataContext;

            this.vm.PropertyChanged += Vm_PropertyChanged;
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(vm.Target):
                    txtTarget.Text = vm.Target;
                    break;
            }
        }

        private void SetHighlighting()
        {
            this.txtSource.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Json");
            this.txtTarget.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Json");
        }

        private void Source_LostFocus(object sender, RoutedEventArgs e)
        {
            this.vm.Source = txtSource.Text;
        }
    }
}