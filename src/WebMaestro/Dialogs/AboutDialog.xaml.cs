using System.Windows;

namespace WebMaestro.Dialogs
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
        
            var version = this.GetType().Assembly.GetName().Version;
            this.txtTitle.Text = string.Format("WebMaestro v{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
