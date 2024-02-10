using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows;
using WebMaestro.ViewModels.Dialogs;

namespace WebMaestro.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for PrettifyTool.xaml
    /// </summary>
    public partial class PrettifyTool : Window
    {
        private PrettifyToolViewModel vm;

        public PrettifyTool()
        {
            InitializeComponent();
            SetHighlighting(PrettifyTypes.Json);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.vm = (PrettifyToolViewModel)this.DataContext;

            this.vm.PropertyChanged += Vm_PropertyChanged;
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(vm.Target):
                    txtTarget.Text = vm.Target;
                    break;
                case nameof(vm.PrettifyType):
                    SetHighlighting(vm.PrettifyType);
                    break;
            }
        }

        private void SetHighlighting(PrettifyTypes prettifyType)
        {
            switch (prettifyType)
            {
                case PrettifyTypes.Json:
                    this.txtSource.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Json");
                    this.txtTarget.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Json");
                    break;
                case PrettifyTypes.XML:
                    this.txtSource.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
                    this.txtTarget.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
                    break;
                default:
                    break;
            }
        }

        private void Source_LostFocus(object sender, RoutedEventArgs e)
        {
            this.vm.Source = txtSource.Text;
        }
    }
}
