using System;
using System.Text.Json.Serialization;
using WebMaestro.ViewModels;

namespace WebMaestro.Models
{
    public class CollectionFileModel
    {
        public string Name { get; set; }

        public string FileName { get; set; }

        public string Url { get; set; }

        public HttpMethods HttpMethod { get; set; }

        [JsonIgnore]
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
