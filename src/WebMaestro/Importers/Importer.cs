using System;
using System.Collections.Generic;
using System.IO;
using WebMaestro.Models;

namespace WebMaestro.Importers
{
    internal abstract class Importer
    {
        public CollectionModel Collection { get; set; } = new();

        public List<RequestModel> Requests { get; } = new List<RequestModel>();

        public abstract void Import(Stream stream);
    }
}
