using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using WebMaestro.Models;
using WebMaestro.ViewModels;
using System.Linq;
using ICSharpCode.AvalonEdit.Search;
using System.Windows.Threading;
using System.Reflection;
using System.Windows.Interop;
using System.Text.Json;

namespace WebMaestro.Views
{
    /// <summary>
    /// Interaction logic for WebView.xaml
    /// </summary>
    public partial class WebView : UserControl
    {
        private WebViewModel vm;

        // to prevent race condition when editing or updating
        // the request body
        private bool isEditing;
        private bool isUpdating;

        public WebView()
        {
            InitializeComponent();

            this.Loaded += WebView_Loaded;
        }

        private void WebView_Loaded(object sender, RoutedEventArgs e)
        {
            InitBindings();

            this.DataContextChanged += WebView_DataContextChanged
                ;
            this.editRequest.TextChanged += EditRequest_TextChanged;

            SearchPanel.Install(this.editRequest);
            SearchPanel.Install(this.editResponse);

            this.editRequest.Text = this.vm.Request.Body;

            // The following disables the Javascript dialog "continue to execute javascript on this page..."
            this.previewResponse.NavigateToString("<html></html>");

            object activeX = this.previewResponse.GetType().InvokeMember("ActiveXInstance",
                BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, this.previewResponse, new object[] { });

            activeX.GetType().InvokeMember("Silent",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public,
                null, activeX, new object[] { true });
        }

        private void WebView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            InitBindings();

            this.editRequest.Text = this.vm.Request.Body;

            this.editResponse.Text = this.vm.SelectedHistoryItem != null ? this.vm.SelectedHistoryItem.Response.Body : "";

            GeneratePreview();
        }

        private void InitBindings()
        {
            if (this.vm != null)
            {
                this.vm.PropertyChanged -= Vm_PropertyChanged;
                this.vm.Request.PropertyChanged -= Request_PropertyChanged;
            }

            this.vm = (WebViewModel)this.DataContext;

            this.vm.PropertyChanged += Vm_PropertyChanged;
            this.vm.Request.PropertyChanged += Request_PropertyChanged;

        }

        private void EditRequest_TextChanged(object sender, EventArgs e)
        {
            if (this.isUpdating)
            {
                return;
            }

            this.isEditing = true;
            this.vm.Request.Body = this.editRequest.Text;
            this.isEditing = false;
            System.Diagnostics.Debug.WriteLine("Editing...");
        }

        private void Request_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.vm.Request.Body))
            {
                if (isEditing)
                {
                    return;
                }

                this.isUpdating = true;
                this.editRequest.Text = this.vm.Request.Body;
                this.isUpdating = false;
                System.Diagnostics.Debug.WriteLine("Updating...");
            }
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.vm.SelectedHistoryItem))
            {
                GeneratePreview();
            }
        }

        private void SetResponseFormat()
        {
            var contentType = this.vm.SelectedHistoryItem != null ? this.vm.SelectedHistoryItem.Response.ContentType : "";

            var oldIndex = cboContentType.SelectedIndex;

            if (string.IsNullOrEmpty(contentType))
            {
                cboContentType.SelectedIndex = 0;
            }
            else if (contentType.Contains("xml"))
            {
                cboContentType.SelectedIndex = 1;
            }
            else if (contentType.Contains("json"))
            {
                cboContentType.SelectedIndex = 2;
            }
            else
            {
                cboContentType.SelectedIndex = 0;
            }

            // If old and new index is the same a change is not triggered
            if (oldIndex == cboContentType.SelectedIndex)
            {
                FormatResponse();
            }
        }

        private void FormatResponse()
        {
            var response = this.vm.SelectedHistoryItem != null ? this.vm.SelectedHistoryItem.Response.Body : "";

            this.editResponse.Document.Text = "";

            var prettyPrint = this.togPrettyPrint.IsChecked.Value;


            if (cboContentType.SelectedIndex == 1)
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
                this.editResponse.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
            }
            else if (cboContentType.SelectedIndex == 2)
            {
                if (prettyPrint)
                {
                    try
                    {
                        var jsonObj = JsonSerializer.Deserialize<object>(response);

                        response = JsonSerializer.Serialize<object>(jsonObj, new JsonSerializerOptions() { WriteIndented = true });
                    }
                    catch { }
                }
                this.editResponse.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Json");
            }
            else
            {
                this.editResponse.SyntaxHighlighting = null;
            }

            this.editResponse.Document.BeginUpdate();
            this.editResponse.Document.Text = response;
            this.editResponse.Document.EndUpdate();
        }

        private void ContentTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            FormatResponse();
        }

        private void GeneratePreview()
        {
            SetResponseFormat();

            if (this.vm.SelectedHistoryItem != null)
            {
                var body = this.vm.SelectedHistoryItem.Response.Body;

                if (string.IsNullOrEmpty(body))
                {
                    previewResponse.NavigateToString("<html><body>No preview is available.</body></html>");
                }
                else
                {
                    previewResponse.NavigateToString(body);
                }
            }
            else
            {
                previewResponse.NavigateToString("<html><body>No preview is available.</body></html>");
            }
        }

        private void PrettyPrint(object sender, RoutedEventArgs e)
        {
            FormatResponse();
        }

        private void RequestContentTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            var contentType = this.vm.Request.Headers.Where(x => string.Equals(x.Name, "content-type", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

            if (contentType != null)
            {
                contentType.Value = ((ComboBoxItem)cboReqContentType.SelectedValue).Content.ToString();
            }
            else
            {
                this.vm.Request.Headers.Add(new HeaderModel("content-type", ((ComboBoxItem)cboReqContentType.SelectedValue).Content.ToString()));
            }
        }

        private void ReqBodyWordWrap(object sender, RoutedEventArgs e)
        {
            editRequest.WordWrap = !editRequest.WordWrap;
        }

        private void RespBodyWordWrap(object sender, RoutedEventArgs e)
        {
            editResponse.WordWrap = !editResponse.WordWrap;
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

        private void ViewCertificate(object sender, RoutedEventArgs e)
        {
            var certificate = this.vm.SelectedHistoryItem.Response.ServerCertificate;

            var win = Window.GetWindow(this);
            var wih = new WindowInteropHelper(win);

            X509Certificate2UI.DisplayCertificate(certificate, wih.Handle);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.vm.SendCommand.Cancel();
        }

        private void AddCertificateUser(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this);
            var wih = new WindowInteropHelper(win);

            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadOnly))
            {
                var certSelection = store.Certificates.Find(X509FindType.FindByApplicationPolicy, "1.3.6.1.5.5.7.3.2", true);
                var certificates = X509Certificate2UI.SelectFromCollection(certSelection,
                    "Certificates for the current user",
                    "Select a certificate to use for client authentication.",
                    X509SelectionFlag.SingleSelection, wih.Handle);

                foreach (var cert in certificates)
                {
                    this.vm.Request.Certificates.Add(cert);
                }
            }
        }

        private void AddCertificateComputer(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this);
            var wih = new WindowInteropHelper(win);

            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine, OpenFlags.ReadOnly))
            {
                var certSelection = store.Certificates.Find(X509FindType.FindByApplicationPolicy, "1.3.6.1.5.5.7.3.2", true);
                var certificates = X509Certificate2UI.SelectFromCollection(certSelection,
                    "Certificates for the local machine",
                    "Select a certificate to use for client authentication.",
                    X509SelectionFlag.SingleSelection, wih.Handle);

                foreach (var cert in certificates)
                {
                    this.vm.Request.Certificates.Add(cert);
                }
            }
        }
    }
}
