using System;

namespace TelegramWebAPI.Models
{
    public class Friendship
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }      // Кто добавил
        public Guid FriendId { get; set; }    // Кого добавили

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}