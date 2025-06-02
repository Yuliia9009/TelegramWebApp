using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramWebAPI.Models.Chat;

namespace TelegramWebAPI.Services.Interfaces
{
    public interface IChatService
    {
        Task<Chat> CreateChatAsync(Chat chat);
        Task<IEnumerable<Chat>> GetChatsForUserAsync(string userId);
        Task<Message> SendMessageAsync(Message message);
        Task<IEnumerable<Message>> GetMessagesAsync(string chatId);
        Task<Message?> GetMessageByIdAsync(string chatId, string messageId);
        Task<Message> UpdateMessageAsync(string messageId, string newText);
        Task<Chat?> GetOrCreatePrivateChatAsync(string user1Id, string user2Id);
    }
}