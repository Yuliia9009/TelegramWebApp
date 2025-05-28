using System.Net;
using Microsoft.Azure.Cosmos;
using TelegramWebAPI.Models.Chat;
using TelegramWebAPI.Services.Interfaces;

namespace TelegramWebAPI.Services
{
    public class ChatMessageRepository : IChatMessageRepository
    {
        private readonly Container _container;

        public ChatMessageRepository(CosmosClient cosmosClient, IConfiguration config)
        {
            var dbName = config["Azure:AzureCosmosDb:DatabaseName"];
            var containerName = config["Azure:AzureCosmosDb:ContainerName"];
            _container = cosmosClient.GetContainer(dbName, containerName);
        }

        public async Task SaveMessageAsync(Message message)
        {
            await _container.CreateItemAsync(message, new PartitionKey(message.ChatId));
        }
        public async Task<Message?> GetMessageByIdAsync(string chatId, string messageId)
        {
            try
            {
                var response = await _container.ReadItemAsync<Message>(messageId, new PartitionKey(chatId));
                return response.Resource;
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }
        public async Task<Message> UpdateMessageAsync(Message message)
        {
            var response = await _container.ReplaceItemAsync(
                message,
                message.Id,
                new PartitionKey(message.ChatId)
            );
            return response.Resource;
        }

        public async Task<Message> UpdateMessageAsync(string chatId, string messageId, string newText)
        {
            var message = await GetMessageByIdAsync(chatId, messageId);
            if (message == null)
            {
                throw new Exception("Message not found");
            }

            message.Text = newText;
            message.EditedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

            return await UpdateMessageAsync(message);
        }
    }
}