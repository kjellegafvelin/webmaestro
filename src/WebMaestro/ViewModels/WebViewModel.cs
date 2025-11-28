using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Linq;
using WebMaestro.Models;
using System.Windows.Data;
using System.Text;
using WebMaestro.Helpers;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using CommunityToolkit.Mvvm.DependencyInjection;
using WebMaestro.ViewModels.Dialogs;
using System.Security.Cryptography;
using System.Windows;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using System.Xml;
using System.Text.Json;
using System.Threading;
using WebMaestro.Core;
using WebMaestro.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WebMaestro.ViewModels
{
    public enum HttpMethods
    {
        GET,
        POST,
        PATCH,
        DELETE,
        PUT,
        OPTIONS,
        HEAD,
        TRACE
    }

    public enum RequestBodyType
    {
        None = 0,
        Form,
        Raw,
        Binary
    }

    public partial class WebViewModel : TabItemViewModel
    {
        private readonly object historyLock = new();
        private static int noOf = 1;
        private readonly IDialogService dialogService;
        private readonly FileService fileService;
        private readonly CollectionsService collectionsService;
        private readonly HttpRequestService requestService;

        public WebViewModel() : this(Guid.NewGuid(), new RequestModel()
        {
            Headers =
                {
                    new HeaderModel("Cache-Control", "no-cache,no-store"),
                    new HeaderModel("Pragma", "no-cache"),
                    new HeaderModel("Accept", "*/*"),
                    new HeaderModel("Accept-Encoding", "gzip, deflate"),
                    new HeaderModel("User-Agent", "WebMaestro")
                }}, Guid.Empty)
        {
        }

        public WebViewModel(Guid id, RequestModel request, Guid collectionId)
        {
            this.Id = id;
            this.dialogService = Ioc.Default.GetRequiredService<IDialogService>();
            this.fileService = Ioc.Default.GetRequiredService<FileService>();
            this.collectionsService = Ioc.Default.GetRequiredService<CollectionsService>();

            this.requestService = new HttpRequestService();

            if (string.IsNullOrEmpty(request.Name))
            {
                request.Name = $"New Request [{ noOf++ }]";
            }
            this.Observer = ModificationObserver.Create(request, collectionId == Guid.Empty);

            base.Name = request.Name;

            this.collection = this.collectionsService.Collections.FirstOrDefault(x => x.Id == collectionId);
            
            this.request = request;
            this.Request.PropertyChanged += Request_PropertyChanged;
            BindingOperations.EnableCollectionSynchronization(this.HistoryItems, this.historyLock);
        }

        private RequestModel request = new();
        public RequestModel Request
        {
            get => this.request;
        }

        private ResponseModel response = new ResponseModel();
        public ResponseModel Response
        {
            get => this.response;
            private set => this.SetProperty(ref this.response, value);
        }

        public ObservableCollection<HistoryItemModel> HistoryItems { get; } = new ObservableCollection<HistoryItemModel>();

        [ObservableProperty]
        private EnvironmentModel selectedEnvironment;

        [ObservableProperty]
        private CollectionModel collection;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private HistoryItemModel selectedHistoryItem;

        [ObservableProperty]
        private HeaderModel selectedRequestHeader;

        [ObservableProperty]
        private CookieModel selectedRequestCookie;

        [ObservableProperty]
        private VariableModel selectedRequestVariable;

        [ObservableProperty]
        private X509Certificate2 selectedRequestCertificate;

        [ObservableProperty]
        private QueryParamModel selectedRequestQueryParam;

        [ObservableProperty]
        private bool validateServerCertificate = true;

        [ObservableProperty]
        private bool allowAutoRedirect = true;

        [ObservableProperty]
        private bool useSession;

        public Dictionary<string, int> Timeouts
        {
            get => new Dictionary<string, int>()
                {
                    { "1s", 1  },
                    { "5s", 5 },
                    { "10s", 10 },
                    { "15s", 15 },
                    { "30s", 30 },
                    { "1 min", 60 },
                    { "2 min", 120 },
                    { "3 min", 180 },
                    { "5 min", 300 },
                    { "10 min", 600 },
                    { "15 min", 900 },
                    { "20 min", 1200 }
                };
        }

        [RelayCommand]
        private void AddHeader()
        {
            var vm = new AddHeaderViewModel();

            if (this.dialogService.ShowDialog(this, vm) == true)
            {
                var header = new HeaderModel(vm.Name, vm.Value)
                {
                    Description = vm.Description
                };

                this.Request.Headers.Add(header);
            }
        }

        [RelayCommand]
        private void EditHeader()
        {
            var vm = new AddHeaderViewModel()
            {
                Name = this.SelectedRequestHeader.Name,
                Value = this.SelectedRequestHeader.Value,
                Description = this.SelectedRequestHeader.Description,
                IsEdit = true
            };

            if (this.dialogService.ShowDialog(this, vm) == true)
            {
                this.SelectedRequestHeader.Value = vm.Value;
                this.SelectedRequestHeader.Description = vm.Description;
            }
        }

        [RelayCommand]
        private void RemoveHeader()
        {
            this.Request.Headers.Remove(this.SelectedRequestHeader);
        }

        [RelayCommand]
        private void AddCookie()
        {
            var vm = new AddCookieViewModel();

            if (this.dialogService.ShowDialog(this, vm) == true)
            {
                var model = new CookieModel(vm.Name, vm.Value);

                this.Request.Cookies.Add(model);
            }
        }

        [RelayCommand]
        private void EditCookie()
        {
            var vm = new AddCookieViewModel()
            {
                Name = this.SelectedRequestCookie.Name,
                Value = this.SelectedRequestCookie.Value,
                IsEdit = true
            };

            if (this.dialogService.ShowDialog(this, vm) == true)
            {
                this.SelectedRequestCookie.Value = vm.Value;
            }
        }

        [RelayCommand]
        private void RemoveCookie()
        {
            this.Request.Cookies.Remove(this.SelectedRequestCookie);
        }

        [RelayCommand]
        private void AddVariable()
        {
            var vm = new AddVariableViewModel();

            if (this.dialogService.ShowDialog(this, vm) == true)
            {
                this.Request.Variables.Add(new VariableModel() { Name = vm.Name, Value = vm.Value });
            }
        }

        [RelayCommand]
        private void EditVariable()
        {
            var vm = new AddVariableViewModel()
            {
                Name = this.SelectedRequestVariable.Name,
                Value = this.SelectedRequestVariable.Value,
                IsEditing = true
            };

            if (this.dialogService.ShowDialog(this, vm) == true)
            {
                this.SelectedRequestVariable.Name = vm.Name;
                this.SelectedRequestVariable.Value = vm.Value;
            }
        }

        [RelayCommand]
        private void RemoveVariable()
        {
            this.Request.Variables.Remove(this.SelectedRequestVariable);
        }

        [RelayCommand]
        private void AddCertificate()
        {
            var vm = new AddCertificateViewModel();

            if (this.dialogService.ShowDialog(this, vm) == true)
            {
                try
                {
                    X509Certificate2 cert;

                    if (vm.HasPassword)
                    {
                        cert = new X509Certificate2(vm.FileName, vm.Password);
                    }
                    else
                    {
                        cert = new X509Certificate2(vm.FileName);
                    }

                    this.Request.Certificates.Add(cert);
                    this.SelectedRequestCertificate = cert;
                }
                catch (CryptographicException ex)
                {
                    this.dialogService.ShowMessageBox(this, ex.Message, "Error opening certificate",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        [RelayCommand]
        private void RemoveCertificate()
        {
            this.Request.Certificates.Remove(this.SelectedRequestCertificate);
        }

        [RelayCommand]
        private void AddQueryParam()
        {
            var vm = new AddQueryParamViewModel();

            if (this.dialogService.ShowDialog(this, vm) == true)
            {
                var param = new QueryParamModel(vm.Key, vm.Value);
                this.Request.QueryParams.Add(param);
                this.SelectedRequestQueryParam = param;
            }
        }

        [RelayCommand]
        private void EditQueryParam()
        {
            var vm = new AddQueryParamViewModel()
            {
                Key = this.SelectedRequestQueryParam.Key,
                Value = this.SelectedRequestQueryParam.Value,
                IsEditing = true
            };

            if (this.dialogService.ShowDialog(this, vm) == true)
            {
                this.SelectedRequestQueryParam.Key = vm.Key;
                this.SelectedRequestQueryParam.Value = vm.Value;
            }
        }

        [RelayCommand]
        private void RemoveQueryParam()
        {
            this.Request.QueryParams.Remove(this.SelectedRequestQueryParam);
        }

        [RelayCommand]
        private void EditComment()
        {
            var vm = new EditCommentViewModel()
            {
                Comment = this.SelectedHistoryItem.Comment
            };

            if (this.dialogService.ShowDialog(this, vm) == true)
            {
                this.SelectedHistoryItem.Comment = vm.Comment;
            }
        }

        [RelayCommand]
        private void RemoveHistoryItem()
        {
            this.HistoryItems.Remove(this.SelectedHistoryItem);
        }

        [RelayCommand]
        private void ClearHistoryItems()
        {
            foreach (var item in this.HistoryItems.Where(x => !x.Keep).ToList())
            {
                this.HistoryItems.Remove(item);
            }
        }

        [RelayCommand]
        private async Task OpenRequestBody()
        {
            var settings = new OpenFileDialogSettings()
            {
                Filter = "All files (*.*)|*.*",
                Title = "Open File"
            };

            if (this.dialogService.ShowOpenFileDialog(this, settings) == true)
            {
                Request.Body = await FileHelpers.OpenFileAsStringAsync(settings.FileName);
            }

        }

        [RelayCommand]
        private async Task SaveResponseBody()
        {
            var settings = new SaveFileDialogSettings()
            {
                Filter = "Text document (*.txt)|*.txt|XML document (*.xml)|*.xml|Json document (*.json)|*.json",
                AddExtension = true,
                //DefaultExt = extension,
                //FileName = $"*.{extension}",
                Title = "Save body to File"
            };

            if (this.dialogService.ShowSaveFileDialog(this, settings) == true)
            {
                await FileHelpers.SaveStringAsFileAsync(settings.FileName, this.SelectedHistoryItem.Response.Body);
            }
        }

        [RelayCommand]
        private void ExtractVariables()
        {
            var startIndex = 0;
            const string startVariable = "${";
            const string endVariable = "}";

            do
            {
                var pos = this.Request.Body.IndexOf(startVariable, startIndex, StringComparison.OrdinalIgnoreCase);

                if (pos == -1 || pos + startVariable.Length >= this.Request.Body.Length)
                {
                    break;
                }

                pos += startVariable.Length;

                var endPos = this.Request.Body.IndexOf(endVariable, pos, StringComparison.OrdinalIgnoreCase);

                if (endPos == -1)
                {
                    break;
                }

                var variable = this.Request.Body[pos..endPos];

                if (!this.Request.Variables.Any(x => x.Name.Equals(variable, StringComparison.OrdinalIgnoreCase)))
                {
                    this.Request.Variables.Add(new VariableModel() { Name = variable });
                }

                startIndex = endPos + endVariable.Length;
            }
            while (startIndex < this.Request.Body.Length);

        }

        [RelayCommand]
        private void PrettifyRequest()
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(this.Request.Body);

                using (var sw = new StringWriter())
                using (var xw = new XmlTextWriter(sw)
                {
                    Formatting = System.Xml.Formatting.Indented
                })
                {
                    xmlDoc.WriteContentTo(xw);
                    xw.Flush();
                    this.Request.Body = sw.ToString();
                }
                return;
            }
            catch (Exception)
            {
            }

            try
            {
                var jsonObj = JsonSerializer.Deserialize<object>(this.Request.Body);

                this.Request.Body = JsonSerializer.Serialize<object>(jsonObj, new JsonSerializerOptions() { WriteIndented = true });
            }
            catch { }

        }

        [RelayCommand]
        private async Task ExportCertificate()
        {
            var settings = new SaveFileDialogSettings()
            {
                Filter = "Certificate file (*.cer)|*.cer",
                AddExtension = true,
                Title = "Save Certificate File"
            };

            if (this.dialogService.ShowSaveFileDialog(this, settings) == true)
            {
                try
                {
                    var certificate = this.SelectedHistoryItem.Response.ServerCertificate;

                    var content = new StringBuilder()
                        .AppendLine("-----BEGIN CERTIFICATE-----")
                        .AppendLine(Convert.ToBase64String(certificate.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks))
                        .AppendLine("-----END CERTIFICATE-----")
                        .ToString();

                    await FileHelpers.SaveStringAsFileAsync(settings.FileName, content);
                }
                catch (Exception)
                {
                    this.dialogService.ShowMessageBox(this, "Failed to save the certificate file!", "File Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void DuplicateHeader(HeaderModel parameter)
        {
            var newHeader = new HeaderModel(parameter.Name, parameter.Value)
            {
                IsEnabled = false
            };

            this.Request.Headers.Add(newHeader);
        }

        [RelayCommand]
        private void CopyHeader(HeaderModel parameter)
        {
            var value = $"{parameter.Name}: {parameter.Value}";

            Clipboard.SetText(value, TextDataFormat.Text);
        }

        [RelayCommand]
        private void CopyHeaderValue(HeaderModel parameter)
        {
            Clipboard.SetText(parameter.Value, TextDataFormat.Text);
        }

        private void Request_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        if (e.PropertyName == nameof(this.Request.Url))
        {
            try
            {
                this.SendCommand.NotifyCanExecuteChanged();
                
                ParseQueryString(this.Request);
            }
            catch (Exception) { }
        }
        }

        [RelayCommand]
        private void Cancel()
        {
            this.SendCommand.Cancel();
        }

        private void ParseQueryString(RequestModel request)
        {
            var uri = new Uri(request.Url);
            var parameters = HttpUtility.ParseQueryString(uri.Query);
            var changed = false;

            if (parameters.Count == request.QueryParams.Count(x => x.IsEnabled))
            {
                foreach (var param in parameters.AllKeys)
                {
                    var count = request.QueryParams.Count(x => x.IsEnabled && x.Key.Equals(param) && x.Value.Equals(parameters.Get(param)));
                    if (count != 1)
                    {
                        changed = true;
                    }
                }
            }
            else
            {
                changed = true;
            }

            if (changed)
            {
                request.QueryParams.Clear();
                foreach (var param in parameters.AllKeys)
                {
                    if (param != null)
                    {
                        request.QueryParams.Add(new QueryParamModel(param, parameters[param]));
                    }
                }
            }
        }


        [RelayCommand]
        private async Task Send(CancellationToken cancellationToken)
        {
            try
            {
                var response = await requestService.SendAsync(this.SelectedEnvironment, this.request, cancellationToken);

                var historyItem = new HistoryItemModel()
                {
                    Response = response,
                    Date = DateTime.Now
                };

                this.HistoryItems.Insert(0, historyItem);

                this.SelectedHistoryItem = historyItem;

            }
            catch (TaskCanceledException)
            {
                // Send was cancelled
            }
            catch (Exception ex)
            {
                this.dialogService.ShowMessageBox(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public override async Task<bool> OnSave()
        {
            if (Collection is null)
            {
                var vm = new SaveRequestViewModel();

                // Prepopulate the name with the URL path (without query params) when available
                try
                {
                    if (!string.IsNullOrWhiteSpace(this.Request.Url))
                    {
                        var uri = new Uri(this.Request.Url);
                        vm.Name = uri.AbsolutePath;
                    }
                }
                catch (Exception)
                {
                    // Ignore invalid URL and let the user provide a name
                }

                if (this.dialogService.ShowDialog(this, vm) != true)
                {
                    return false;
                }

#pragma warning disable MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
                collection = vm.Collection;
#pragma warning restore MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
                Name = Request.Name = vm.Name;
            }

            await this.fileService.SaveRequest(Collection, this.Id, request);
            await this.collectionsService.SaveCollectionAsync(Collection);

            return true;
        }

        [RelayCommand]
        private void Test()
        {
            var vm = new TestRunnerViewModel(this.SelectedEnvironment, this.request);

            _ = this.dialogService.ShowDialog(this, vm);
        }

        [RelayCommand]
        private void OpenFileForRequest()
        {
            var settings = new OpenFileDialogSettings()
            {
                Filter = "All files (*.*)|*.*",
                Title = "Open File"
            };

            if (this.dialogService.ShowOpenFileDialog(this, settings) == true)
            {
                Request.Filename = settings.FileName;
            }

        }

    }
}

