using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using WebMaestro.ViewModels;

namespace WebMaestro.Models
{
    public class CollectionContext
    {
        public string Name { get; set; }

        public string Filename { get; set; }
    }

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

    public partial class EnvironmentModel : ObservableObject
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string url;

        public ObservableCollection<VariableModel> Variables { get; set; } = new();
    }

    public class CollectionFileModel
    {
        public string Name { get; set; }

        public string FileName { get; set; }

        public string Url { get; set; }

        public HttpMethods HttpMethod {get; set; }

        [JsonIgnore]
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
