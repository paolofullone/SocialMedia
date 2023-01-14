using CQRS.Core.Domain;
using CQRS.Core.Events;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Post.Cmd.Infrastructure.Config;

namespace Post.Cmd.Infrastructure.Repositories
{
    public class EventStoreRepository : IEventStoreRepository
    {
        private readonly IMongoCollection<EventModel> _eventStoreCollection; // EventModel é o schema que definimos para salvar as collections.

        public EventStoreRepository(IOptions<MongoDbConfig> config)
        {
            var mongoClient = new MongoClient(config.Value.ConnectionString); // aqui vamos usar a configuração que injetamos no IOptions do MongoDbConfig. 
            // Passamos a config no appSettings.Development.json e injetamos no IOptions do MongoDbConfig na Program.cs.
            var mongoDatabase = mongoClient.GetDatabase(config.Value.Database);

            _eventStoreCollection = mongoDatabase.GetCollection<EventModel>(config.Value.Collection);
        }

        public async Task<List<EventModel>> FindAllAsync()
        {
            return await _eventStoreCollection.Find(_ => true).ToListAsync().ConfigureAwait(false); // discard operator because we are finding everything.
        }

        public async Task<List<EventModel>> FindByAggregateId(Guid aggregateId)
        {
            return await _eventStoreCollection.Find(x => x.AggregateIdentifier == aggregateId).ToListAsync().ConfigureAwait(false);
            // ConfigureAwait(false) is used to tell the compiler that the continuation can be run on any thread and avoid forcing the calllback to be
            // invoked on the original context or scheduler
            // the benefits are improving performance and avoiding deadlocks for example.
        }

        public async Task SaveAsync(EventModel @event)
        {
            await _eventStoreCollection.InsertOneAsync(@event).ConfigureAwait(false);
        }
    }
}

// Importante notar que como os eventos são imutáveis, não criamos update nem delete neste repositório. Somente um SaveAsync e um FindByAggregateId.