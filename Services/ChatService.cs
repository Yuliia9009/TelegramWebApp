using Microsoft.Azure.Cosmos;
using TelegramWebAPI.Models.Chat;
using TelegramWebAPI.Services.Interfaces;
using TelegramWebAPI.Data;
using Microsoft.AspNetCore.SignalR;
using TelegramWebAPI.Hubs;

namespace TelegramWebAPI.Services
{
    public class ChatService : IChatService
    {
        private readonly Container _chatContainer;
        private readonly Container _messageContainer;
        private readonly CosmosDbService _cosmosDbService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatService(CosmosDbService cosmosDbService, IHubContext<ChatHub> hubContext)
        {
            _cosmosDbService = cosmosDbService;
            _chatContainer = cosmosDbService.GetChatContainer();
            _messageContainer = cosmosDbService.GetMessageContainer();
            _hubContext = hubContext;
        }

        public async Task<Chat> CreateChatAsync(Chat chat)
        {
            try
            {
                var response = await _chatContainer.CreateItemAsync(chat, new PartitionKey(chat.Id));
                return response.Resource;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"❌ Ошибка создания чата: {ex.Message}");
                throw;
            }
        }

        public async Task<Message> SendMessageAsync(Message message)
        {
            if (string.IsNullOrWhiteSpace(message.ChatId))
                throw new ArgumentException("ChatId не может быть пустым при отправке сообщения.");

            if (string.IsNullOrWhiteSpace(message.Id))
                message.Id = Guid.NewGuid().ToString();

            if (message.SentAt == default)
                message.SentAt = DateTime.UtcNow;

            Console.WriteLine("📤 Сохраняем сообщение:");
            Console.WriteLine($"  Id: {message.Id}");
            Console.WriteLine($"  ChatId: {message.ChatId}");
            Console.WriteLine($"  SenderId: {message.SenderId}");
            Console.WriteLine($"  Text: {message.Text}");

            try
            {
                var response = await _messageContainer.CreateItemAsync(message, new PartitionKey(message.ChatId));

                // 📢 Рассылаем сообщение всем участникам чата через SignalR
                await _hubContext.Clients.Group(message.ChatId)
                    .SendAsync("ReceiveMessage", response.Resource);

                return response.Resource;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"❌ Ошибка сохранения сообщения: {ex.Message}");
                throw;
            }
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

        public async Task<Message> UpdateMessageAsync(string chatId, string messageId, string newText)
        {
            var message = await GetMessageByIdAsync(chatId, messageId);

            if (message == null)
                throw new Exception("Message not found");

            message.Text = newText;
            message.EditedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

            try
            {
                var response = await _messageContainer.ReplaceItemAsync(
                    message,
                    message.Id,
                    new PartitionKey(chatId)
                );

                return response.Resource;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"❌ Ошибка обновления сообщения: {ex.Message}");
                throw;
            }
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

        public async Task<Chat> GetOrCreateChatAsync(string userId1, string userId2)
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.IsGroup = false AND ARRAY_CONTAINS(c.Participants, @user1Id) AND ARRAY_CONTAINS(c.Participants, @user2Id)"
            )
            .WithParameter("@user1Id", userId1)
            .WithParameter("@user2Id", userId2);

            var iterator = _chatContainer.GetItemQueryIterator<Chat>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                foreach (var chat in response)
                {
                    return chat;
                }
            }

            var newChat = new Chat
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"Чат между {userId1} и {userId2}",
                IsGroup = false,
                Participants = new List<string> { userId1, userId2 },
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId1
            };

            await _chatContainer.CreateItemAsync(newChat, new PartitionKey(newChat.Id));
            return newChat;
        }

        public Task<Chat?> GetOrCreatePrivateChatAsync(string user1Id, string user2Id)
        {
            return GetOrCreateChatAsync(user1Id, user2Id);
        }

        public async Task<Chat> GetChatByIdAsync(string chatId)
        {
            return await _cosmosDbService.GetChatByIdAsync(chatId);
        }
    }
}