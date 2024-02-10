using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace WebMaestro.Models
{
    
    public partial class CookieModel : ObservableObject
    {
        public CookieModel()
        {
        }

        public CookieModel(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string value;

        [ObservableProperty]
        private string path;

        [ObservableProperty]
        private string domain;

        public DateTime Expires { get; internal set; }
        public bool HttpOnly { get; internal set; }
        public bool Secure { get; internal set; }
    }
}

