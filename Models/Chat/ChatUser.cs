using System;

using TelegramWebAPI.Models.Chat;
using TelegramWebAPI.Models;

namespace TelegramWebAPI.Models.Chat
{
    public class ChatUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string ChatId { get; set; }
        public Guid UserId { get; set; }

        public Chat Chat { get; set; }
        public User User { get; set; }
    }
}