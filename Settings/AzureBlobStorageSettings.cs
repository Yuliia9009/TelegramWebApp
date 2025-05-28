using TelegramWebAPI.Settings;

namespace TelegramWebAPI.Settings
{
    public class AzureBlobStorageSettings
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
    }
}