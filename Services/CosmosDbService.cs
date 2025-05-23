using Microsoft.Azure.Cosmos;
using TelegramWebAPI.Models;

namespace TelegramWebAPI.Services
{
    public class CosmosDbService
    {
        private readonly Container _container;

        public CosmosDbService(IConfiguration config)
        {
            var connectionString = config["Azure:AzureCosmosDb:ConnectionString"];
            var databaseName = config["Azure:AzureCosmosDb:DatabaseName"];
            var containerName = config["Azure:AzureCosmosDb:ContainerName"];

            var client = new CosmosClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _container = database.GetContainer(containerName);
        }

        public async Task<TestMessage> SaveTestMessageAsync()
        {
            var message = new TestMessage(); 
            await _container.CreateItemAsync(message, new PartitionKey(message.chatId));
            return message;
        }
    }
}