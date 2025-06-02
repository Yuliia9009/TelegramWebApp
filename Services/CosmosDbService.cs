using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using TelegramWebAPI.Models;
using TelegramWebAPI.Settings;

namespace TelegramWebAPI.Services
{
    public class CosmosDbService
    {
        private readonly Container _messageContainer;
        private readonly Container _chatContainer;

        public CosmosDbService(IOptions<AzureCosmosDbSettings> options)
        {
            var settings = options.Value;

            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new ArgumentNullException(nameof(settings.ConnectionString));
            if (string.IsNullOrWhiteSpace(settings.DatabaseName))
                throw new ArgumentNullException(nameof(settings.DatabaseName));
            if (string.IsNullOrWhiteSpace(settings.ContainerName))
                throw new ArgumentNullException(nameof(settings.ContainerName));

            var client = new CosmosClient(settings.ConnectionString);
            var database = client.CreateDatabaseIfNotExistsAsync(settings.DatabaseName).Result;

            database.Database.CreateContainerIfNotExistsAsync(settings.ContainerName, "/chatId").Wait();
            _messageContainer = database.Database.GetContainer(settings.ContainerName);

            database.Database.CreateContainerIfNotExistsAsync("Chats", "/id").Wait();
            _chatContainer = database.Database.GetContainer("Chats");
        }

        public Container GetMessageContainer() => _messageContainer;
        public Container GetChatContainer() => _chatContainer;

        public async Task<TestMessage> SaveTestMessageAsync()
        {
            var message = new TestMessage();
            await _messageContainer.CreateItemAsync(message, new PartitionKey(message.chatId));
            return message;
        }
    }
}