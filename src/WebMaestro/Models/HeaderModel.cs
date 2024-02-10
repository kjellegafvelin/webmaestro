using CommunityToolkit.Mvvm.ComponentModel;
using WebMaestro.Core;

namespace WebMaestro.Models
{

    public class HeaderModel : ObservableObject
    {

        public HeaderModel()
        {

        }

        public HeaderModel(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public HeaderModel(string name, string value, string description) : this(name, value)
        {
            this.description = description;
        }

        private string name;
        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        private string value;
        public string Value
        {
            get => this.value;
            set => this.SetProperty(ref this.value, value);
        }

        private bool isEnabled = true;
        public bool IsEnabled
        {
            get => this.isEnabled;
            set => this.SetProperty(ref this.isEnabled, value);
        }

        private string description;
        public string Description 
        { 
            get => this.description;
            set => this.SetProperty(ref this.description, value);
        }

    }
}

