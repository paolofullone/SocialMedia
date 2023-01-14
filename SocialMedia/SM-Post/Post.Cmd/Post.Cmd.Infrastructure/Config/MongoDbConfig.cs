namespace Post.Cmd.Infrastructure.Config
{
    public class MongoDbConfig
    {
        public string? ConnectionString { get; set; }
        public string? Database { get; set; }
        public string? Collection { get; set; }
    }
}

// passamos no appSettings.Development.json do projeto API a configuração do mongo conforme esta classe.