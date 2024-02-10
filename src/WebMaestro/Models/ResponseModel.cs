using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;

namespace WebMaestro.Models
{
    public partial class ResponseModel : ModelBase
    {
        private string url = string.Empty;
        private string contentType = string.Empty;
        private string body = string.Empty;
        private X509Certificate2 serverCertificate;
        private HttpStatusCode status;
        private TimeSpan elapsed;
        private long size;
        private string reason;

        [ObservableProperty]
        private string httpVersion;

        public string Url
        {
            get => this.url;
            set => this.SetProperty(ref this.url, value);
        }

        public string Body
        {
            get => body;
            set => this.SetProperty(ref this.body, value);
        }

        public ObservableCollection<CookieModel> Cookies { get; } = new ObservableCollection<CookieModel>(); 
        
        public ObservableCollection<HeaderModel> Headers { get; } = new ObservableCollection<HeaderModel>();

        public ResponseModel()
        {
        }

        public ResponseModel(ResponseModel response)
        {
            this.url = response.url;
            this.body = response.body;
            this.contentType = response.contentType;
            this.Cookies = new ObservableCollection<CookieModel>(response.Cookies);
            this.Headers = new ObservableCollection<HeaderModel>(response.Headers);
        }

        public string ContentType
        {
            get => this.contentType;
            set
            {
                this.SetProperty(ref this.contentType, value);
            }
        }

        [JsonIgnore]
        public long Size
        {
            get { return size; }
            internal set { this.SetProperty(ref size, value); }
        }

        [JsonIgnore]
        public TimeSpan Elapsed
        {
            get { return elapsed; }
            internal set { this.SetProperty(ref elapsed, value); }
        }

        [JsonIgnore]
        public HttpStatusCode Status
        {
            get { return status; }
            internal set { this.SetProperty(ref status, value); }
        }

        [JsonIgnore]
        public X509Certificate2 ServerCertificate
        {
            get { return serverCertificate; }
            internal set { this.SetProperty(ref serverCertificate, value); }
        }

        [JsonIgnore]
        public string Reason
        {
            get { return reason; }
            internal set { this.SetProperty(ref reason, value); }
        }
    }
}
