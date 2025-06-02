using Microsoft.Azure.Cosmos;
using TelegramWebAPI.Models.Chat;
using TelegramWebAPI.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace TelegramWebAPI.Services
{
    public class ChatService : IChatService
    {
        private readonly Container _chatContainer;
        private readonly Container _messageContainer;

        public ChatService(CosmosDbService cosmosDbService)
        {
            _chatContainer = cosmosDbService.GetChatContainer();
            _messageContainer = cosmosDbService.GetMessageContainer();
        }

        public async Task<Chat> CreateChatAsync(Chat chat)
        {
            var response = await _chatContainer.CreateItemAsync(chat, new PartitionKey(chat.Id));
            return response.Resource;
        }

        public async Task<Message> SendMessageAsync(Message message)
        {
            var response = await _messageContainer.CreateItemAsync(message, new PartitionKey(message.ChatId));
            return response.Resource;
        }

        public async Task<IEnumerable<Message>> GetMessagesAsync(string chatId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.ChatId = @chatId")
                .WithParameter("@chatId", chatId);

            var result = new List<Message>();
            var iterator = _messageContainer.GetItemQueryIterator<Message>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                result.AddRange(response);
            }

            return result;
        }

        public async Task<Message?> GetMessageByIdAsync(string chatId, string messageId)
        {
            try
            {
                var response = await _messageContainer.ReadItemAsync<Message>(messageId, new PartitionKey(chatId));
                return response.Resource;
            }
            catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<Message> UpdateMessageAsync(string messageId, string newText)
        {
            var message = await GetMessageByIdAsync("your-chat-id-here", messageId); // chatId можно поправить при вызове

            if (message == null)
                throw new Exception("Message not found");

            message.Text = newText;
            message.EditedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

            var response = await _messageContainer.ReplaceItemAsync(
                message,
                message.Id,
                new PartitionKey(message.ChatId)
            );

            return response.Resource;
        }

        public async Task<IEnumerable<Chat>> GetChatsForUserAsync(string userId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE ARRAY_CONTAINS(c.Participants, @userId)")
                .WithParameter("@userId", userId);

            var result = new List<Chat>();
            var iterator = _chatContainer.GetItemQueryIterator<Chat>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                result.AddRange(response);
            }

            return result;
        }

        public async Task<Chat?> GetOrCreatePrivateChatAsync(string user1Id, string user2Id)
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.IsGroup = false AND ARRAY_CONTAINS(c.Participants, @user1Id) AND ARRAY_CONTAINS(c.Participants, @user2Id)"
            )
            .WithParameter("@user1Id", user1Id)
            .WithParameter("@user2Id", user2Id);

            var iterator = _chatContainer.GetItemQueryIterator<Chat>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                foreach (var chat in response)
                {
                    return chat;
                }
            }

            return null;
        }
    }
}