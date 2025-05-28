using TelegramWebAPI.Settings; 

namespace TelegramWebAPI.Settings
{
    public class AzureCosmosDbSettings
    {
        public string ConnectionString { get; set; }
        public string PrimaryKey { get; set; }
        public string DatabaseName { get; set; }
        public string ContainerName { get; set; }
    }
}
