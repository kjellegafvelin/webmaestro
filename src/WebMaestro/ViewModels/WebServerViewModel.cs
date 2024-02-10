using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using WebMaestro.Core;
using WebMaestro.Helpers;
using WebMaestro.Models;
using WebMaestro.ViewModels.Dialogs;

namespace WebMaestro.ViewModels
{
    internal class WebServerViewModel : TabItemViewModel
    {
        private static int noOf = 1;
        private HttpListener listener = new HttpListener();
        private readonly IDialogService dialogService;

        private readonly object historyLock = new object();

        public WebServerViewModel()
        {
            base.Name = $"New Mock [{ noOf++ }]";

            this.IsStarted = false;

            BindingOperations.EnableCollectionSynchronization(this.HistoryItems, this.historyLock);

            this.dialogService = Ioc.Default.GetRequiredService<IDialogService>();
            this.Observer = ModificationObserver.Create(this.Response);
        }

        public ObservableCollection<X509Certificate2> Certificates { get; set; } = new();

        public Dictionary<string, int> Delays
        {
            get => new Dictionary<string, int>()
                {
                    { "0", 0 },
                    { "1s", 1  },
                    { "5s", 5 },
                    { "10s", 10 },
                    { "15s", 15 },
                    { "30s", 30 },
                    { "1min", 60 },
                    { "2min", 120 },
                    { "3min", 180 },
                    { "5min", 300 }
                };
        }

        public int Delay { get; set; }

        private bool isNotStarted;
        public bool IsNotStarted
        {
            get => this.isNotStarted;
        }

        private bool isStarted;
        public bool IsStarted
        {
            get => this.isStarted;
            set
            {
                SetProperty(ref this.isStarted, value);
                SetProperty(ref this.isNotStarted, !value, nameof(IsNotStarted));
            }
        }

        private int status = 200;

        public int Status
        {
            get => this.status;
            set => SetProperty(ref this.status, value);
        }

        private TimeSpan elapsed;
        public TimeSpan Elapsed
        {
            get => this.elapsed;
            private set => SetProperty(ref this.elapsed, value);
        }

        private long size;
        public long Size
        {
            get => this.size;
            private set => SetProperty(ref this.size, value);
        }

        private string errorText;
        public string ErrorText
        {
            get => this.errorText;
            private set => SetProperty(ref this.errorText, value);
        }

        private RequestModel request = new();
        public RequestModel Request
        {
            get => this.request;
            private set => this.SetProperty(ref this.request, value);
        }

        private ResponseModel response = new();
        public ResponseModel Response
        {
            get => this.response;
            internal set => this.SetProperty(ref this.response, value);
        }

        public ObservableCollection<HistoryItemModel> HistoryItems { get; } = new();

        private bool transferEncodingChunked;
        public bool TransferEncodingChunked
        {
            get => this.transferEncodingChunked;
            set => this.SetProperty(ref this.transferEncodingChunked, value);
        }

        private HeaderModel selectedResponseHeader;
        public HeaderModel SelectedResponseHeader
        {
            get => selectedResponseHeader;
            set => SetProperty(ref selectedResponseHeader, value);
        }

        private RelayCommand addHeaderCommand;
        public RelayCommand AddHeaderCommand => addHeaderCommand ??= new RelayCommand(AddHeader);

        private void AddHeader()
        {
            var vm = new AddHeaderViewModel();

            if (this.dialogService.ShowDialog(this, vm) == true)
            {
                var header = new HeaderModel(vm.Name, vm.Value)
                {
                    Description = vm.Description
                };

                this.Response.Headers.Add(header);
            }
        }

        private RelayCommand editHeaderCommand;
        public RelayCommand EditHeaderCommand => editHeaderCommand ??= new RelayCommand(EditHeader);

        private void EditHeader()
        {
            var vm = new AddHeaderViewModel()
            {
                Name = this.SelectedResponseHeader.Name,
                Value = this.SelectedResponseHeader.Value,
                Description = this.SelectedResponseHeader.Description,
                IsEdit = true
            };

            if (this.dialogService.ShowDialog(this, vm) == true)
            {
                this.SelectedResponseHeader.Value = vm.Value;
                this.SelectedResponseHeader.Description = vm.Description;
            }
        }

        private RelayCommand removeHeaderCommand;
        public RelayCommand RemoveHeaderCommand => removeHeaderCommand ??= new RelayCommand(RemoveHeader);

        private void RemoveHeader()
        {
            this.Response.Headers.Remove(this.SelectedResponseHeader);
        }

        private ICommand startCommand;
        public ICommand StartCommand => startCommand ??= new RelayCommand(Start);

        public void Start()
        {
            try
            {
                this.ErrorText = "";

                if (string.IsNullOrWhiteSpace(this.Response.Url))
                {
                    this.ErrorText = "Server URL is missing. Specify the server URL like 'http://localhost:9111/' or http://localhost/customers/.'";
                    return;
                }


                if (!this.Response.Url.EndsWith("/"))
                {
                    this.Response.Url += "/";
                }

                this.listener.Prefixes.Add(this.Response.Url);
                this.listener.Start();
                this.listener.BeginGetContext(new System.AsyncCallback(ListenerCallback), listener);

                this.IsStarted = true;
            }
            catch (Exception ex)
            {
                this.ErrorText = ex.Message;
            }
        }

        private ICommand stopCommand;
        public ICommand StopCommand => stopCommand ??= new RelayCommand(Stop);

        internal void Stop()
        {
            this.listener.Stop();
            this.IsStarted = false;
            this.listener = new HttpListener();
        }

        private void ListenerCallback(IAsyncResult result)
        {
            var listener = (HttpListener)result.AsyncState;
            HttpListenerContext context;

            if (!this.listener.IsListening)
            {
                return;
            }

            try
            {
                context = this.listener.EndGetContext(result);
            }
            catch (HttpListenerException ex)
            {
                // Listener is stopped
                if (ex.ErrorCode == 995)
                {
                    return;
                }

                throw;
            }
            var request = context.Request;
            var response = context.Response;

            this.Request = new RequestModel();

            if (request.HasEntityBody)
            {
                using (var sr = new StreamReader(request.InputStream))
                {
                    var requestBody = sr.ReadToEnd();
                    this.Request.Body = requestBody;
                }
            }

            var cert = request.GetClientCertificate();

            if (cert != null)
            {
                this.Request.Certificates.Add(cert);
            }

            this.Request.Url = request.Url.ToString();
            this.Request.HttpMethod = (HttpMethods)Enum.Parse(typeof(HttpMethods), request.HttpMethod, true);
            this.Request.ContentType = request.ContentType;

            foreach (var name in request.QueryString.AllKeys)
            {
                this.Request.QueryParams.Add(new QueryParamModel(name, request.QueryString[name]));
            }

            foreach (var name in request.Headers.AllKeys)
            {
                this.Request.Headers.Add(new HeaderModel(name, request.Headers[name]));
            }

            this.HistoryItems.Insert(0, new HistoryItemModel()
            {
                Request = this.Request,
                Date = DateTime.Now
            });

            if (this.Delay > 0)
            {
                Thread.Sleep(this.Delay * 1000);
            }

            try
            {
                response.Headers.Remove("Server");
                response.AppendHeader("Server", "WebMaestro 1.0 Server");

                foreach (var header in this.Response.Headers)
                {

                    switch (header.Name.ToLower())
                    {
                        case "content-type":
                            response.ContentType = header.Value;
                            break;
                        case "keep-alive":
                            response.KeepAlive = header.Value.Equals("true", StringComparison.OrdinalIgnoreCase);
                            break;
                        case "transfer-encoding":
                            response.SendChunked = header.Value.Equals("chunked", StringComparison.OrdinalIgnoreCase);
                            break;
                        case "content-encoding":
                            break;
                        case "content-length":
                            break;
                        default:
                            response.AppendHeader(header.Name, header.Value);
                            break;
                    }

                }

                foreach (var cookie in this.Response.Cookies)
                {
                    response.SetCookie(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));

                }

                var buffer = Encoding.UTF8.GetBytes(this.response.Body);

                if (!this.transferEncodingChunked)
                {
                    response.ContentLength64 = buffer.Length;
                }

                response.ContentEncoding = Encoding.UTF8;
                response.ContentType = this.Response.ContentType;
                response.StatusCode = this.status;
                response.StatusDescription = Enum.GetName(typeof(HttpStatusCode), this.status);

                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
                response.Close();
            }
            catch (HttpListenerException)
            {

            }
            this.listener.BeginGetContext(new System.AsyncCallback(ListenerCallback), listener);

        }

        internal async Task SaveRequest(string fileName, RequestModel request)
        {
            await FileHelpers.SaveJsonFileAsync(fileName, request);
        }

        internal async Task SaveResponse(string fileName)
        {
            await FileHelpers.SaveJsonFileAsync(fileName, this.Response);

            this.response.IsDirty = false;
        }

        public override async Task<bool> OnSave()
        {
            var settings = new SaveFileDialogSettings()
            {
                Filter = "Response file (*.resp)|*.resp",
                AddExtension = true,
                Title = "Save Response File"
            };

            if (this.dialogService.ShowSaveFileDialog(this, settings) != true)
            {
                return false;
            }

            await this.SaveResponse(settings.FileName);
            return true;
        }
    }
}
