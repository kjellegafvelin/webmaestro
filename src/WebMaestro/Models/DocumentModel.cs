using System;
using System.Text.Json.Serialization;
using WebMaestro.ViewModels;

namespace WebMaestro.Models
{
    internal class DocumentModel
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string File { get; set; }
    }
}
