using System;

namespace WebMaestro.Models
{
    public class HistoryItemModel : ModelBase
    {
        public RequestModel Request { get; set; }

        public ResponseModel Response { get; set; }

        public DateTime Date { get; set; }

        private bool keep;
        public bool Keep { get => this.keep; set => this.SetProperty(ref this.keep, value); }

        private string comment;
        public string Comment { get => this.comment; set => this.SetProperty(ref this.comment, value); }

    }
}
