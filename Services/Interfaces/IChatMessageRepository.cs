using TelegramWebAPI.Models.Chat;

namespace TelegramWebAPI.Services.Interfaces
{
    public interface IChatMessageRepository
    {
        Task SaveMessageAsync(Message message);
        Task<Message?> GetMessageByIdAsync(string chatId, string messageId);
        Task<Message> UpdateMessageAsync(Message message); 
    }
}