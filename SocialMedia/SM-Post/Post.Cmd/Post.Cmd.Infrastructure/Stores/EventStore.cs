using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Exceptions; // criamos uma pasta com a exception
using CQRS.Core.Infrastructure;
using CQRS.Core.Producers;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Infrastructure.Stores
{
    public class EventStore : IEventStore
    {
        private readonly IEventStoreRepository _eventStoreRepository;
        private readonly IEventProducer _eventProducer;

        public EventStore(IEventStoreRepository eventStoreRepository, IEventProducer eventProducer)
        {
            _eventStoreRepository = eventStoreRepository;
            _eventProducer = eventProducer;
        }

        public async Task<List<Guid>> GetAggregateIdsAsync()
        {
            var eventStream = await _eventStoreRepository.FindAllAsync();

            if (eventStream == null || !eventStream.Any())
                throw new ArgumentNullException(nameof(eventStream), "Could not retrieve event stream from the event store");

            return eventStream.Select(x => x.AggregateIdentifier).Distinct().ToList();

        }

        public async Task<List<BaseEvent?>> GetEventsAsync(Guid aggregateId)
        {
            var eventStream = await _eventStoreRepository.FindByAggregateId(aggregateId);

            if (eventStream == null || !eventStream.Any())
                throw new AggregateNotFoundException("Incorrect post ID provided!"); // post Id � equivalente ao aggregateId

            return eventStream.OrderBy(x => x.Version).Select(x => x.EventData).ToList();
        }

        public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<BaseEvent> events, int expectedVersion)
        {
            var eventStream = await _eventStoreRepository.FindByAggregateId(aggregateId);

            // validar essas op��es...
            // if (eventStream != null && eventStream.Any() && eventStream.Select(x => x.Version).Max() != expectedVersion)
            // eventStream[^1].Version is the same of eventStream.Last().Version or (eventStream.Length - 1).Version
            if (expectedVersion != -1 && eventStream[^1].Version != expectedVersion)
                throw new ConcurrencyException();

            var version = expectedVersion;

            foreach (var @event in events)
            {
                version++; // incrementa a vers�o
                @event.Version = version; // seta a vers�o no evento
                var eventType = @event.GetType().Name; // pega o nome do evento
                var eventModel = new EventModel // cria um objeto do tipo EventModel
                {
                    TimeStamp = DateTime.Now,
                    AggregateIdentifier = aggregateId,
                    AggregateType = nameof(PostAggregate),
                    Version = version,
                    EventType = eventType,
                    EventData = @event
                };

                await _eventStoreRepository.SaveAsync(eventModel); // m�todo ass�ncrono

                var topic = "SocialMediaPostEvents";
                //var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC"); 
                // pega o nome do t�pico do Kafka
                // if you want to deploy your microservices to Docker, you can also add the environmental variables to the docker-compose.yml file
                // or if you want to deploy to kubernetes you can add the environmental variables to the deployment.yml file

                // salvou uma vari�vel de ambiente no launch.json que est� na pasta .vscode no "env" do Post.Query.Api e no Post.Cmd.Api

                await _eventProducer.ProduceAsync(topic!, @event); // lembrando que o @event est� dentro do foreach
            }
        }
    }
}