using Confluent.Kafka;
using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using CQRS.Core.Producers;
using MongoDB.Bson.Serialization;
using Post.Cmd.Api.Commands;
using Post.Cmd.Domain.Aggregates;
using Post.Cmd.Infrastructure.Config;
using Post.Cmd.Infrastructure.Dispatchers;
using Post.Cmd.Infrastructure.Handlers;
using Post.Cmd.Infrastructure.Producers;
using Post.Cmd.Infrastructure.Repositories;
using Post.Cmd.Infrastructure.Stores;
using Post.Common.Events;

var builder = WebApplication.CreateBuilder(args);

// to fix the abstract class intantiation error we had on put comment, the BaseEvent is an abstract class and MongoDB in it's default state does not support
// polimorphism.
BsonClassMap.RegisterClassMap<BaseEvent>();
BsonClassMap.RegisterClassMap<PostCreatedEvent>();
BsonClassMap.RegisterClassMap<MessageUpdatedEvent>();
BsonClassMap.RegisterClassMap<PostLikedEvent>();
BsonClassMap.RegisterClassMap<CommentAddedEvent>();
BsonClassMap.RegisterClassMap<CommentUpdatedEvent>();
BsonClassMap.RegisterClassMap<CommentRemovedEvent>();
BsonClassMap.RegisterClassMap<PostRemovedEvent>();

// Add services to the container.

builder.Services.Configure<MongoDbConfig>(builder.Configuration.GetSection(nameof(MongoDbConfig))); // essa linha vai injetar as configurações definidas
// no appsetings.development.json no IOptions do MongoDbConfig que depois vamos usar na classe EventStoreRepository.

builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection(nameof(ProducerConfig)));

builder.Services.AddScoped<IEventStoreRepository, EventStoreRepository>(); // essa linha vai injetar a classe EventStoreRepository
// Scoped service will create a new instance for each unique http request.
// Singleton service will create a single instance for the entire application.
// Transient service will create a new instance everywhere we use it.

builder.Services.AddScoped<IEventProducer, EventProducer>(); // we want to inject a new instance of event producer with every unique http request

builder.Services.AddScoped<IEventStore, EventStore>(); // injetando o serviço da classe EventSourcingHandler

builder.Services.AddScoped<IEventSourcingHandler<PostAggregate>, EventSourcingHandler>(); // injetando o serviço da classe EventSourcingHandler

builder.Services.AddScoped<ICommandHandler, CommandHandler>(); // injetando o serviço da classe CommandHandler

// a ordem de registro dos serviços importa aqui, pois a IEventStoreRepository depende do Mongo, a IEventStore depende da IEventStoreRepository...

// register comand handler methods
var comandHandler = builder.Services.BuildServiceProvider().GetRequiredService<ICommandHandler>(); // como não estamos criando os serviços como singleton, não precisamos
// nos preocupar com este warning aqui.
var dispatcher = new CommandDispatcher();
dispatcher.RegisterHandler<NewPostCommand>(comandHandler.HandleAsync);
dispatcher.RegisterHandler<EditMessageCommand>(comandHandler.HandleAsync);
dispatcher.RegisterHandler<LikePostCommand>(comandHandler.HandleAsync);
dispatcher.RegisterHandler<AddCommentCommand>(comandHandler.HandleAsync);
dispatcher.RegisterHandler<EditCommentCommand>(comandHandler.HandleAsync);
dispatcher.RegisterHandler<RemoveCommentCommand>(comandHandler.HandleAsync);
dispatcher.RegisterHandler<DeletePostCommand>(comandHandler.HandleAsync);
dispatcher.RegisterHandler<RestoreReadDbCommand>(comandHandler.HandleAsync);
builder.Services.AddSingleton<ICommandDispatcher>(_ => dispatcher); // CommandDispatcher vai preencher o dicionário com os comandos enviados acima.


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

//using Confluent.Kafka;
//using CQRS.Core.Domain;
//using CQRS.Core.Events;
//using CQRS.Core.Handlers;
//using CQRS.Core.Infrastructure;
//using CQRS.Core.Producers;
//using MongoDB.Bson.Serialization;
//using Post.Cmd.Api.Commands;
//using Post.Cmd.Domain.Aggregates;
//using Post.Cmd.Infrastructure.Config;
//using Post.Cmd.Infrastructure.Dispatchers;
//using Post.Cmd.Infrastructure.Handlers;
//using Post.Cmd.Infrastructure.Producers;
//using Post.Cmd.Infrastructure.Repositories;
//using Post.Cmd.Infrastructure.Stores;
//using Post.Common.Events;

//var builder = WebApplication.CreateBuilder(args);

//BsonClassMap.RegisterClassMap<BaseEvent>();
//BsonClassMap.RegisterClassMap<PostCreatedEvent>();
//BsonClassMap.RegisterClassMap<MessageUpdatedEvent>();
//BsonClassMap.RegisterClassMap<PostLikedEvent>();
//BsonClassMap.RegisterClassMap<CommentAddedEvent>();
//BsonClassMap.RegisterClassMap<CommentUpdatedEvent>();
//BsonClassMap.RegisterClassMap<CommentRemovedEvent>();
//BsonClassMap.RegisterClassMap<PostRemovedEvent>();

//// Add services to the container.
//builder.Services.Configure<MongoDbConfig>(builder.Configuration.GetSection(nameof(MongoDbConfig)));
//builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection(nameof(ProducerConfig)));
//builder.Services.AddScoped<IEventStoreRepository, EventStoreRepository>();
//builder.Services.AddScoped<IEventProducer, EventProducer>();
//builder.Services.AddScoped<IEventStore, EventStore>();
//builder.Services.AddScoped<IEventSourcingHandler<PostAggregate>, EventSourcingHandler>();
//builder.Services.AddScoped<ICommandHandler, CommandHandler>();

//// register command handler methods
//var commandHandler = builder.Services.BuildServiceProvider().GetRequiredService<ICommandHandler>();
//var dispatcher = new CommandDispatcher();
//dispatcher.RegisterHandler<NewPostCommand>(commandHandler.HandleAsync);
//dispatcher.RegisterHandler<EditMessageCommand>(commandHandler.HandleAsync);
//dispatcher.RegisterHandler<LikePostCommand>(commandHandler.HandleAsync);
//dispatcher.RegisterHandler<AddCommentCommand>(commandHandler.HandleAsync);
//dispatcher.RegisterHandler<EditCommentCommand>(commandHandler.HandleAsync);
//dispatcher.RegisterHandler<RemoveCommentCommand>(commandHandler.HandleAsync);
//dispatcher.RegisterHandler<DeletePostCommand>(commandHandler.HandleAsync);
//builder.Services.AddSingleton<ICommandDispatcher>(_ => dispatcher);

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();
