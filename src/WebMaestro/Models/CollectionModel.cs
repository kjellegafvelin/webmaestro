using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace WebMaestro.Models
{

    public partial class CollectionModel : ObservableObject
    {
        [ObservableProperty]
        private string name;

        public ObservableCollection<CollectionFileModel> Files { get; set; } = new();

        public ObservableCollection<EnvironmentModel> Environments { get; set; } = new();

        public ObservableCollection<VariableModel> Variables { get; set; } = new();

        [JsonIgnore]
        public string Path { get; internal set; }

        [JsonIgnore]
        public Guid Id { get; } = Guid.NewGuid();
    }
}
