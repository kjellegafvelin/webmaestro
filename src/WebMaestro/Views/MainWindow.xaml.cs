using Fluent;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WebMaestro.Controls;
using WebMaestro.Dialogs;
using WebMaestro.Helpers;
using WebMaestro.Models;
using WebMaestro.ViewModels;

namespace WebMaestro.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        private readonly MainViewModel vm;

        public MainWindow()
        {
            InitializeComponent();

            this.vm = (MainViewModel)this.DataContext;

            this.tabs.TabItemClosing += async (s, e) => {
                TabItemViewModel vm = ((TabItemViewModel)e.TabItem.Content);

                if (!vm.Observer.IsModified)
                {
                    return;
                }

                var result = MessageBox.Show("Save the changes made to the request?", "Save Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        this.vm.SelectedTabItem = vm;
                        await vm.Save();
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        e.Cancel = true;
                        break;
                }
            };
        }

        private void OpenRequestExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Request file (*.req)|*.req";
            dlg.Title = "Open Request File";

            var win = Window.GetWindow(this);
            if (dlg.ShowDialog(win) ?? true)
            {
                win.IsEnabled = false;
                win.Cursor = Cursors.Wait;

                try
                {
                    var req = FileHelpers.ReadJsonFile<RequestModel>(dlg.FileName);
                    //NewRequest(req);
                }
                catch (Exception)
                {
                    MessageBox.Show(win, "Failed to open the request file!", "File Open Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                win.IsEnabled = true;
                win.Cursor = Cursors.Arrow;
            }
        }

        private void OpenResponseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Response file (*.resp)|*.resp";
            dlg.Title = "Open Response File";

            var win = Window.GetWindow(this);
            if (dlg.ShowDialog(win) ?? true)
            {
                win.IsEnabled = false;
                win.Cursor = Cursors.Wait;

                try
                {
                    var resp = FileHelpers.ReadJsonFile<ResponseModel>(dlg.FileName);
                    //NewServer(resp);
                }
                catch (Exception)
                {
                    MessageBox.Show(win, "Failed to open the response file!", "File Open Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                win.IsEnabled = true;
                win.Cursor = Cursors.Arrow;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AboutDialog();
            dlg.Show();
        }

        private void CheckForUpdates(object sender, RoutedEventArgs e)
        {
            OpenWebsite("Https://getwebmaestro.com/download");
        }

        private void VisitWebsite(object sender, RoutedEventArgs e)
        {
            OpenWebsite("Https://getwebmaestro.com");
        }

        private void GotoSupport(object sender, RoutedEventArgs e)
        {
            OpenWebsite("https://twitter.com/kjellegafvelin");
        }

        private void SaveToCollectionExecuted(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void SaveToCollectionCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void SendRequestExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //if (!TryGetViewModel(out ViewModelBase vm))
            //{
            //    return;
            //}

            //if (vm is WebViewModel webViewModel)
            //{
            //    await webViewModel.Send();
            //}
        }

        private void SendRequestCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //if (!TryGetViewModel(out ViewModelBase vm))
            //{
            //    return;
            //}

            //if (vm is WebViewModel webViewModel)
            //{
            //    e.CanExecute = !string.IsNullOrEmpty(webViewModel.Request.Url) && webViewModel.IsNotSending;
            //}
            //else if (vm is WebServerViewModel _)
            //{
            //    e.CanExecute = false;
            //}

            e.Handled = true;
            e.ContinueRouting = false;
        }

        private void CancelRequestExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //if (!TryGetViewModel(out ViewModelBase vm))
            //{
            //    return;
            //}

            //if (vm is WebViewModel webViewModel)
            //{
            //    //await webViewModel.Cancel();
            //}

        }

        private void CancelRequestCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //if (!TryGetViewModel(out ViewModelBase vm))
            //{
            //    return;
            //}

            //if (vm is WebViewModel webViewModel)
            //{
            //    e.CanExecute = webViewModel.IsSending;
            //}
            //else if (vm is WebServerViewModel _)
            //{
            //    e.CanExecute = false;
            //}

            e.Handled = true;
            e.ContinueRouting = false;
        }

        private void StartServerExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //if (!TryGetViewModel(out ViewModelBase vm))
            //{
            //    return;
            //}

            //if (vm is WebServerViewModel webServerViewModel)
            //{
            //    webServerViewModel.Start();
            //}
        }

        private void StartServerCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //if (!TryGetViewModel(out ViewModelBase vm))
            //{
            //    return;
            //}

            //if (vm is WebViewModel _)
            //{
            //    e.CanExecute = false;
            //}
            //else if (vm is WebServerViewModel webServerViewModel)
            //{
            //    e.CanExecute = !string.IsNullOrEmpty(webServerViewModel.Response.Url);
            //}

            e.Handled = true;
            e.ContinueRouting = false;

        }

        private void StopServerExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //if (!TryGetViewModel(out ViewModelBase vm))
            //{
            //    return;
            //}

            //if (vm is WebServerViewModel webServerViewModel)
            //{
            //    webServerViewModel.Stop();
            //}
        }

        private void StopServerCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //if (!TryGetViewModel(out ViewModelBase vm))
            //{
            //    return;
            //}

            //if (vm is WebViewModel _)
            //{
            //    e.CanExecute = false;
            //}
            //else if (vm is WebServerViewModel _)
            //{
            //    e.CanExecute = true;
            //}

            e.Handled = true;
            e.ContinueRouting = false;

        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            OpenWebsite("https://getwebmaestro.com");
        }

        private static void OpenWebsite(string url)
        {
            var ps = new ProcessStartInfo(url)
            {
                UseShellExecute = true,
                Verb = "open"
            };

            Process.Start(ps);
        }
    }
}
