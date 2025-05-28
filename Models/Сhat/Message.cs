using System;

namespace TelegramWebAPI.Models.Chat
{
    public class Message
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ChatId { get; set; }
        public string SenderId { get; set; }
        public string Text { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsRead { get; set; } = false;
        public string? EditedAt { get; set; }      // время редактирования (если было)
        public MessageType Type { get; set; } = MessageType.Text; // тип сообщения
    }
}
