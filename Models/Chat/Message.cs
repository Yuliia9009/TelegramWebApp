using System;
using Newtonsoft.Json;

namespace TelegramWebAPI.Models.Chat
{
    using Newtonsoft.Json;

        public class Message
        {
            [JsonProperty("id")]
            public string Id { get; set; } = Guid.NewGuid().ToString();

            [JsonProperty("chatId")]
            public string ChatId { get; set; }

            [JsonProperty("senderId")]
            public string SenderId { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("sentAt")]
            public DateTime SentAt { get; set; } = DateTime.UtcNow;

            public DateTime? DeliveredAt { get; set; }
            public DateTime? ReadAt { get; set; }
            public bool IsRead { get; set; } = false;

            [JsonProperty("editedAt")]
            public string? EditedAt { get; set; }

            [JsonProperty("type")]
            public MessageType Type { get; set; } = MessageType.Text;
        }
}