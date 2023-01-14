using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CQRS.Core.Events
{
    public class EventModel
    {
        [BsonId] // � o id do mongo
        [BsonRepresentation(BsonType.ObjectId)] // para o mongo gerar o id
        public string? Id { get; set; } // � o id do evento, n�o do aggregate
        public DateTime TimeStamp { get; set; } // data e hora do evento
        public Guid AggregateIdentifier { get; set; } // id do aggregate
        public string? AggregateType { get; set; } // tipo do aggregate
        public int Version { get; set; } // vers�o do aggregate
        public string? EventType { get; set; } // tipo do evento
        public BaseEvent? EventData { get; set; } // dados do evento
    }
}