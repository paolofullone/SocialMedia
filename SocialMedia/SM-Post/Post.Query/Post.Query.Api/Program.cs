using Confluent.Kafka;
using CQRS.Core.Consumers;
using CQRS.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Post.Query.Api.Queries;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;
using Post.Query.Infrastructure.Consumers;
using Post.Query.Infrastructure.DataAccess;
using Post.Query.Infrastructure.Dispatchers;
using Post.Query.Infrastructure.Handlers;
using Post.Query.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// the lazyloadingproxies will allow us to return our navigation properties, in other words, we will be able to return the comments on a post.

// Using MSSQL:
//Action<DbContextOptionsBuilder> configureDbContext = (
//    options => options.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"))
//    );

// USING POSTGRES
Action<DbContextOptionsBuilder> configureDbContext = (
    options => options.UseLazyLoadingProxies().UseNpgsql(builder.Configuration.GetConnectionString("PGServer"))
    );

builder.Services.AddDbContext<DatabaseContext>(configureDbContext);
//builder.Services.AddSingleton<DatabaseContextFactory>(new DatabaseContextFactory(configureDbContext));
builder.Services.AddSingleton(new DatabaseContextFactory(configureDbContext)); // same as above.

// create database and tables from code
var dataContext = builder.Services.BuildServiceProvider().GetRequiredService<DatabaseContext>();
dataContext.Database.EnsureCreated();

// registrando o eventHandler.
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IQueryHandler, QueryHandler>(); // query handler depends on IPostRepository, so it should be after the register of it.
builder.Services.AddScoped<IEventHandler, Post.Query.Infrastructure.Handlers.EventHandler>(); // se digitar só eventHandler, o compilador aponta erro
// ctrl + . => fully qualify vai ter 2 opções, system e a post.query...

// registrating the event consumer
builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection(nameof(ConsumerConfig)));
builder.Services.AddScoped<IEventConsumer, EventConsumer>();

// register query handler methods
var queryHandler = builder.Services.BuildServiceProvider().GetRequiredService<IQueryHandler>();
var dispatcher = new QueryDispatcher();
dispatcher.RegisterHandler<FindAllPostsQuery>(queryHandler.HandleAsync);
dispatcher.RegisterHandler<FindPostByIdQuery>(queryHandler.HandleAsync);
dispatcher.RegisterHandler<FindPostsByAuthorQuery>(queryHandler.HandleAsync);
dispatcher.RegisterHandler<FindPostsWithCommentsQuery>(queryHandler.HandleAsync);
dispatcher.RegisterHandler<FindPostsWithLikesQuery>(queryHandler.HandleAsync);
builder.Services.AddSingleton<IQueryDispatcher<PostEntity>>(_ => dispatcher);


builder.Services.AddControllers();
builder.Services.AddHostedService<ConsumerHostedService>(); // esse registro vai inicializar a classe ConsumerHostedService que tem o método StartAsync que vai 
// dizer ao event consumer para começar a consumir.

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
//using CQRS.Core.Consumers;
//using Microsoft.EntityFrameworkCore;
//using Post.Query.Domain.Repositories;
//using Post.Query.Infrastructure.Consumers;
//using Post.Query.Infrastructure.DataAccess;
//using Post.Query.Infrastructure.Handlers;
//using Post.Query.Infrastructure.Repositories;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//Action<DbContextOptionsBuilder> configureDbContext = o => o.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
//builder.Services.AddDbContext<DatabaseContext>(configureDbContext);
//builder.Services.AddSingleton<DatabaseContextFactory>(new DatabaseContextFactory(configureDbContext));

//// create database and tables
//var dataContext = builder.Services.BuildServiceProvider().GetRequiredService<DatabaseContext>();
//dataContext.Database.EnsureCreated();

//builder.Services.AddScoped<IPostRepository, PostRepository>();
//builder.Services.AddScoped<ICommentRepository, CommentRepository>();
//builder.Services.AddScoped<IEventHandler, Post.Query.Infrastructure.Handlers.EventHandler>();
//builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection(nameof(ConsumerConfig)));
//builder.Services.AddScoped<IEventConsumer, EventConsumer>();

//builder.Services.AddControllers();
//builder.Services.AddHostedService<ConsumerHostedService>();

//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
