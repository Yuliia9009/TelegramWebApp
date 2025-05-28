using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using TelegramWebAPI.Models;
using TelegramWebAPI.Settings;

namespace TelegramWebAPI.Services
{
    public class CosmosDbService
    {
        private readonly Container _container;

        public CosmosDbService(IOptions<AzureCosmosDbSettings> options)
        {
            var settings = options.Value;

            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new ArgumentNullException(nameof(settings.ConnectionString), "❌ Не указан ConnectionString");

            if (string.IsNullOrWhiteSpace(settings.DatabaseName))
                throw new ArgumentNullException(nameof(settings.DatabaseName), "❌ Не указано имя базы данных");

            if (string.IsNullOrWhiteSpace(settings.ContainerName))
                throw new ArgumentNullException(nameof(settings.ContainerName), "❌ Не указано имя контейнера");

            var client = new CosmosClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _container = database.GetContainer(settings.ContainerName);
        }

        public async Task<TestMessage> SaveTestMessageAsync()
        {
            var message = new TestMessage();
            await _container.CreateItemAsync(message, new PartitionKey(message.chatId));
            return message;
        }
    }
}