using System.Text.Json;
using Confluent.Kafka;
using CQRS.Core.Events;
using CQRS.Core.Producers;
using Microsoft.Extensions.Options;

namespace Post.Cmd.Infrastructure.Producers
{
    public class EventProducer : IEventProducer
    {
        private readonly ProducerConfig _config;

        public EventProducer(IOptions<ProducerConfig> config) // o ProducerConfig vai injetar a configuração do bootstrapserver definido no appsettings.json
        {
            _config = config.Value;
        }

        public async Task ProduceAsync<T>(string topic, T @event) where T : BaseEvent
        {
            using var producer = new ProducerBuilder<string, string>(_config) // key and value type, duas strings
                .SetKeySerializer(Serializers.Utf8)
                .SetValueSerializer(Serializers.Utf8)
                .Build(); // o kafka producer vai ser criado com a configuração do bootstrapserver, passou o using para que ele possa ser liberado da memória após o uso.

            var eventMessage = new Message<string, string> // a mensagem deve ter os mesmos tipos de key e value do producer.
            {
                Key = Guid.NewGuid().ToString(), // o key é o id do evento 
                Value = JsonSerializer.Serialize(@event, @event.GetType()) // o value é o evento serializado
                // GetType() is very important, because we are using polymorphism, so we need to know the type of the event.
                // this complies to the liskov substitution principle, because we are using the base class as the type.
            };

            var deliveryResult = await producer.ProduceAsync(topic, eventMessage);

            if (deliveryResult.Status == PersistenceStatus.NotPersisted)
            {
                throw new Exception($"Could not produce {@event.GetType().Name} message to topic - {topic} due to the following reason: {deliveryResult.Message}.");
            }
        }
    }
}