using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using WebMaestro.Core;
using WebMaestro.ViewModels;

namespace WebMaestro.Models
{
    public enum AuthenticationTypes
    {
        None,
        Basic,
        ApiKey,
        BearerToken
    }

    public enum ApiKeyLocations
    {
        Header,
        Querystring
    }

    public class Authentication : ModelBase
    {
        private AuthenticationTypes type;

        public AuthenticationTypes Type
        {
            get => this.type;
            set => this.SetProperty(ref this.type, value);
        }

        private string key;
        public string Key
        {
            get => this.key;
            set => this.SetProperty(ref this.key, value);
        }

        private string value;
        public string Value
        {
            get => this.value;
            set => this.SetProperty(ref this.value, value);
        }

        private string username;
        public string Username
        {
            get => this.username;
            set => this.SetProperty(ref this.username, value);
        }

        private string password;
        public string Password
        {
            get => this.password;
            set => this.SetProperty(ref this.password, value);
        }

        private ApiKeyLocations apiKeyLocation;
        public ApiKeyLocations ApiKeyLocation
        {
            get => this.apiKeyLocation;
            set => this.SetProperty(ref this.apiKeyLocation, value);
        }

        private string token;
        public string Token
        {
            get => this.token;
            set => this.SetProperty(ref this.token, value);
        }
    }

    public class HttpsProtocols : ModelBase
    {
        public HttpsProtocols()
        {
            this.UseDefault = true;
        }
        private bool useDefault;

        public bool UseDefault
        {
            get { return useDefault; }
            set { this.SetProperty(ref useDefault, value); }
        }

        private bool useSsl20;

        public bool UseSsl20
        {
            get { return useSsl20; }
            set { this.SetProperty(ref useSsl20, value); }
        }

        private bool useSsl30;

        public bool UseSsl30
        {
            get { return useSsl30; }
            set { this.SetProperty(ref useSsl30, value); }
        }

        private bool useTls10;

        public bool UseTls10
        {
            get { return useTls10; }
            set { this.SetProperty(ref useTls10, value); }
        }

        private bool useTls11;

        public bool UseTls11
        {
            get { return useTls11; }
            set { this.SetProperty(ref useTls11, value); }
        }

        private bool useTls12;

        public bool UseTls12
        {
            get { return useTls12; }
            set { this.SetProperty(ref useTls12, value); }
        }

        private bool useTls13;

        public bool UseTls13
        {
            get { return useTls13; }
            set { this.SetProperty(ref useTls13, value); }
        }

    }

    public partial class RequestModel : ObservableValidator
    {
        public RequestModel()
        {

        }

        public RequestModel(RequestModel request)
        {
            this.url = request.url;
            this.httpMethod = request.httpMethod;
            this.body = request.body;
            this.Headers = new (request.Headers);
            this.QueryParams = new (request.QueryParams);
        }

        private string name = string.Empty;
        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        [ObservableProperty]
        private string url = string.Empty;

        private HttpMethods httpMethod;
        public HttpMethods HttpMethod
        {
            get => httpMethod;
            set => this.SetProperty(ref this.httpMethod, value);
        }

        private string body;
        public string Body
        {
            get => body;
            set => this.SetProperty(ref this.body, value);
        }

        private Authentication authentication = new();
        public Authentication Authentication
        {
            get => this.authentication;
            private set => this.SetProperty(ref this.authentication, value);
        }

        public ObservableCollection<HeaderModel> Headers { get; set; } = new();

        public ObservableCollection<CookieModel> Cookies { get; set; } = new();

        public ObservableCollection<VariableModel> Variables { get; set; } = new();

        public ObservableCollection<QueryParamModel> QueryParams { get; } = new();

        [JsonIgnore]
        public ObservableCollection<X509Certificate2> Certificates { get; } = new();


        private string contentType;
        public string ContentType
        {
            get => this.contentType;
            internal set => this.SetProperty(ref this.contentType, value);
        }

        private HttpsProtocols httpsProtocols = new();

        public HttpsProtocols HttpsProtocols
        {
            get { return httpsProtocols; }
            set { this.SetProperty(ref httpsProtocols, value); }
        }

        [ObservableProperty]
        private string filename;

        [ObservableProperty]
        private RequestBodyType bodyType = RequestBodyType.None;

        [ObservableProperty]
        public int timeout = 120;

        [ObservableProperty]
        private HttpVersion httpVersion = HttpVersion.AUTO;

        [ObservableProperty]
        private bool httpVersionExact;

    }

    public enum HttpVersion
    {
        AUTO = 0,
        HTTP11,
        HTTP20
    }

    public class VariableModel : ObservableObject, IEditableObject
    {
        private bool oldIsEnabled;
        private string oldName;
        private string oldValue;
        private string oldDescription;
        private string value;
        private string name;
        private bool isEnabled;
        private string description;

        public VariableModel()
        {
        }

        public VariableModel(string name, string value, string description)
        {
            this.name = name;
            this.value = value;
            this.description = description;
        }

        public VariableModel(string name, string value, string description, bool enabled) : this (name, value, description)
        {
            this.isEnabled = enabled;
        }

        public bool IsEnabled { get => isEnabled; set => this.SetProperty(ref this.isEnabled, value); }

        public string Name { get => name; set => this.SetProperty(ref this.name, value); }

        public string Value { get => value; set => this.SetProperty(ref this.value, value); }

        public string Description { get => this.description; set => this.SetProperty(ref this.description, value); }

        public void BeginEdit()
        {
            oldIsEnabled = IsEnabled;
            oldName ??= Name;
            oldValue ??= Value;
            oldDescription ??= Description;
        }

        public void CancelEdit()
        {
            IsEnabled = oldIsEnabled;
            Name = oldName;
            Value = oldValue;
            Description = oldDescription;
        }

        public void EndEdit()
        {
            oldIsEnabled = false;
            oldName = null;
            oldValue = null;
            oldDescription = null;
        }
    }
}
