using System.Windows;
using WebMaestro.ViewModels.Dialogs;

namespace WebMaestro.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for ImportRawHttp.xaml
    /// </summary>
    public partial class ImportRawHttp : Window
    {
        private ImportRawHttpViewModel vm;

        public ImportRawHttp()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.vm = (ImportRawHttpViewModel)this.DataContext;
        }

        private void Source_LostFocus(object sender, RoutedEventArgs e)
        {
            this.vm.Source = txtSource.Text;
        }

    }
}
