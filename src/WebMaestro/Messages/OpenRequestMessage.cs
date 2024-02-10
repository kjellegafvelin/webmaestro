using System;

namespace WebMaestro.Messages
{
    internal record OpenRequestMessage
    {
        public System.Guid Id { get; init; }

        public string Filename { get; init; }

        public Guid CollectionId { get; init; }
    }
}
