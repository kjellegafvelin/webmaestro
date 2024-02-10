using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using WebMaestro.Dialogs;
using WebMaestro.Models;
using WebMaestro.ViewModels;

namespace WebMaestro.Views
{
    /// <summary>
    /// Interaction logic for WebView.xaml
    /// </summary>
    public partial class WebServerView : UserControl
    {
        private WebServerViewModel vm;

        public WebServerView()
        {
            InitializeComponent();

            this.Loaded += WebServerView_Loaded;
        }

        private void WebServerView_Loaded(object sender, RoutedEventArgs e)
        {
            this.vm = (WebServerViewModel)this.DataContext;

            SearchPanel.Install(this.editRequest);
            SearchPanel.Install(this.editResponse);

            this.editResponse.TextChanged += EditResponse_TextChanged;
            this.editResponse.Text = this.vm.Response.Body;
        }

        private void EditResponse_TextChanged(object sender, EventArgs e)
        {
            this.vm.Response.Body = this.editResponse.Text;
        }

        private async void SaveRequestExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(e.Parameter is RequestModel request))
            {
                throw new ArgumentNullException(nameof(e.Parameter));
            }

            var dlg = new SaveFileDialog();
            dlg.Filter = "Request file (*.req)|*.req";
            dlg.AddExtension = true;
            dlg.Title = "Save Request File";

            var win = Window.GetWindow(this);
            if (dlg.ShowDialog(win) ?? true)
            {
                win.IsEnabled = false;
                win.Cursor = Cursors.Wait;

                try
                {
                    await this.vm.SaveRequest(dlg.FileName, request);
                }
                catch (Exception)
                {
                    MessageBox.Show(win, "Failed to save the request file!", "File Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                win.IsEnabled = true;
                win.Cursor = Cursors.Arrow;
            }
        }

        private void OpenResponseBody(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "All files (*.*)|*.*";
            dlg.Title = "Open File";

            var win = Window.GetWindow(this);
            if (dlg.ShowDialog(win) ?? true)
            {
                using (var s = dlg.OpenFile())
                {
                    editResponse.Load(s);
                    s.Close();
                }
            }
        }

        private void SaveRequestBody(object sender, RoutedEventArgs e)
        {
            string[] extensions = { "txt", "xml", "json" };
            var extension = extensions[cboReqContentType.SelectedIndex];

            var dlg = new SaveFileDialog();
            dlg.Filter = "Text document (*.txt)|*.txt|XML document (*.xml)|*.xml|Json document (*.json)|*.json";
            dlg.AddExtension = true;
            dlg.DefaultExt = extension;
            dlg.FileName = $"*.{extension}";
            dlg.Title = "Save body to File";

            if (dlg.ShowDialog(Window.GetWindow(this)).Value)
            {
                using (var s = dlg.OpenFile())
                {
                    editRequest.Save(s);
                    s.Close();
                }
            }
        }

        private void HistoryItemSelected(object sender, SelectionChangedEventArgs e)
        {
            //editRequest.Text = lvwHistory.SelectedItem == null ? "" : ((HistoryItemModel)lvwHistory.SelectedItem).Request.Body;
            SetRequestFormat();
        }

        private void ClearRequests(object sender, RoutedEventArgs e)
        {
            foreach (var item in this.vm.HistoryItems.Where(x => !x.Keep).ToList())
            {
                this.vm.HistoryItems.Remove(item);
            }
        }

        private void SetRequestFormat()
        {
            var contentType = (HistoryItemModel)lvwHistory.SelectedItem != null ? ((HistoryItemModel)lvwHistory.SelectedItem).Request.ContentType : "";

            var oldIndex = cboReqContentType.SelectedIndex;

            if (string.IsNullOrEmpty(contentType))
            {
                cboReqContentType.SelectedIndex = 0;
            }
            else if (contentType.Contains("xml"))
            {
                cboReqContentType.SelectedIndex = 1;
            }
            else if (contentType.Contains("json"))
            {
                cboReqContentType.SelectedIndex = 2;
            }
            else if (contentType.Contains("x-www-form-urlencoded"))
            {
                cboReqContentType.SelectedIndex = 3;
            }
            else
            {
                cboReqContentType.SelectedIndex = 0;
            }

            // If old and new index is the same a change is not triggered
            if (oldIndex == cboReqContentType.SelectedIndex)
            {
                FormatRequest();
            }
        }

        private void FormatRequest()
        {
            var response = (HistoryItemModel)lvwHistory.SelectedItem != null ? ((HistoryItemModel)lvwHistory.SelectedItem).Request.Body : "";

            this.editRequest.Document.Text = "";
            var prettyPrint = togPrettyPrint.IsChecked.Value;

            if (cboReqContentType.SelectedIndex == 1)
            {
                if (prettyPrint)
                {
                    try
                    {
                        var xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(response);

                        using (var sw = new StringWriter())
                        using (var xw = new XmlTextWriter(sw)
                        {
                            Formatting = System.Xml.Formatting.Indented
                        })
                        {
                            xmlDoc.WriteContentTo(xw);
                            xw.Flush();
                            response = sw.ToString();
                        }
                    }
                    catch { }
                }

                this.editRequest.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
            }
            else if (cboReqContentType.SelectedIndex == 2)
            {
                if (prettyPrint)
                {
                    try
                    {
                        var jsonObj = JsonSerializer.Deserialize<object>(response);

                        response = JsonSerializer.Serialize<Object>(jsonObj, new JsonSerializerOptions() { WriteIndented = true });
                    }
                    catch { }
                }

                this.editRequest.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Json");
            }
            else
            {
                this.editRequest.SyntaxHighlighting = null;
            }

            this.editRequest.Document.BeginUpdate();
            this.editRequest.Document.Text = response ?? string.Empty;
            this.editRequest.Document.EndUpdate();
        }

        private void ContentTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            FormatRequest();
        }

        private void PrettyPrint(object sender, RoutedEventArgs e)
        {
            FormatRequest();
        }

        private void ReqBodyWordWrap(object sender, RoutedEventArgs e)
        {
            editRequest.WordWrap = !editRequest.WordWrap;
        }

        private void RespBodyWordWrap(object sender, RoutedEventArgs e)
        {
            editResponse.WordWrap = !editResponse.WordWrap;
        }

        private void PrettyPrintResponse(object sender, RoutedEventArgs e)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(this.editResponse.Text);

                using (var sw = new StringWriter())
                using (var xw = new XmlTextWriter(sw)
                {
                    Formatting = System.Xml.Formatting.Indented
                })
                {
                    xmlDoc.WriteContentTo(xw);
                    xw.Flush();
                    this.editResponse.Text = sw.ToString();
                }
                return;
            }
            catch { }

            try
            {
                var jsonObj = JsonSerializer.Deserialize<object>(this.editResponse.Text);

                this.editResponse.Text = JsonSerializer.Serialize<object>(jsonObj, new JsonSerializerOptions() { WriteIndented = true });
            }
            catch { }

        }

        private void ResponseContentTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.editResponse == null || cboContentType.SelectedItem == null)
            {
                return;
            }

            var text = this.editResponse.Document.Text;

            if (cboContentType.SelectedItem.ToString().EndsWith("json", StringComparison.InvariantCultureIgnoreCase))
            {
                this.editResponse.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Json");
            }
            else if (cboContentType.SelectedItem.ToString().EndsWith("xml", StringComparison.InvariantCultureIgnoreCase))
            {
                this.editResponse.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
            }
            else if (cboContentType.SelectedItem.ToString().EndsWith("html", StringComparison.InvariantCultureIgnoreCase))
            {
                this.editResponse.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("HTML");
            }
            else
            {
                this.editResponse.SyntaxHighlighting = null;
            }

            this.editResponse.Document.BeginUpdate();
            this.editResponse.Document.Text = text;
            this.editResponse.Document.EndUpdate();
        }

        private void SearchRequestBody(object sender, RoutedEventArgs e)
        {
            var textArea = this.editRequest.TextArea;

            OpenSearchPanel(textArea);
        }

        private void SearchResponseBody(object sender, RoutedEventArgs e)
        {
            var textArea = this.editResponse.TextArea;

            OpenSearchPanel(textArea);
        }

        private static void OpenSearchPanel(ICSharpCode.AvalonEdit.Editing.TextArea textArea)
        {
            var sp = SearchPanel.Install(textArea);
            sp.Open();

            if (!(textArea.Selection.IsEmpty || textArea.Selection.IsMultiline))
            {
                sp.SearchPattern = textArea.Selection.GetText();
            }

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Input, (Action)delegate { sp.Reactivate(); });
        }
    }
}
