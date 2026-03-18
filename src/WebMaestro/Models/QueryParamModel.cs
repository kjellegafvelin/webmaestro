using CommunityToolkit.Mvvm.ComponentModel;

namespace WebMaestro.Models
{

    public class QueryParamModel : ObservableObject
    {
        private bool isEnabled = true;
        private string key;
        private string value;
        private string description;

        public QueryParamModel()
        {
            this.key = string.Empty;
            this.value = string.Empty;
            this.description = string.Empty;
        }

        public QueryParamModel(string key, string value)
        {
            this.key = key;
            this.value = value;
            this.description = string.Empty;
        }

        public QueryParamModel(string key, string value, string description) : this(key, value)
        {
            this.description = description;
        }

        public QueryParamModel(string key, string value, string description, bool isEnabled) : this(key, value, description)
        {
            this.isEnabled = isEnabled;
        }

        public string Key { get => this.key; set => this.SetProperty(ref this.key, value); }
        
        public string Value { get => this.value; set => this.SetProperty(ref this.value, value); }

        public string Description { get => this.description; set => this.SetProperty(ref this.description, value); }

        public bool IsEnabled { get => this.isEnabled; set => this.SetProperty(ref this.isEnabled, value); }

    }
}

