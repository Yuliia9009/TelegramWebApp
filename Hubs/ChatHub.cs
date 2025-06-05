using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TelegramWebAPI.Data;
using TelegramWebAPI.Models.Chat;
using TelegramWebAPI.Services.Interfaces;

namespace TelegramWebAPI.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatMessageRepository _messageRepository;
        private readonly ApplicationDbContext _dbContext;

        public ChatHub(IChatMessageRepository messageRepository, ApplicationDbContext dbContext)
        {
            _messageRepository = messageRepository;
            _dbContext = dbContext;
        }

        public async Task JoinChat(string chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
            Console.WriteLine($"👥 Подключение {Context.ConnectionId} присоединилось к чату {chatId}");
        }

        public async Task LeaveChat(string chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
        }

        public async Task SendMessageToChat(string chatId, Message message)
        {
            message.ChatId = chatId;
            message.SentAt = DateTime.UtcNow;
            message.IsRead = false;

            // Определяем тип сообщения
            if (!string.IsNullOrWhiteSpace(message.Text))
                message.Type = MessageType.Text;
            else if (message.Text?.EndsWith(".mp3") == true)
                message.Type = MessageType.Audio;
            else if (message.Text?.EndsWith(".mp4") == true)
                message.Type = MessageType.Video;
            else if (message.Text?.EndsWith(".jpg") == true || message.Text?.EndsWith(".png") == true)
                message.Type = MessageType.Image;
            else
                message.Type = MessageType.File;

            await _messageRepository.SaveMessageAsync(message);
            await Clients.Group(chatId).SendAsync("ReceiveMessage", message);
        }

        public async Task Typing(string chatId, string userId)
        {
            await Clients.Group(chatId).SendAsync("UserTyping", userId);
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("🔌 OnConnectedAsync вызван");

            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"🆔 Пользователь: {userId ?? "❌ не найден"}");

            if (Guid.TryParse(userId, out var guid))
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == guid);
                if (user != null)
                {
                    user.IsOnline = true;
                    await _dbContext.SaveChangesAsync();

                    // 🔔 Уведомить всех, что пользователь онлайн
                    await Clients.All.SendAsync("UserCameOnline", userId); // или
                    await Clients.All.SendAsync("UserWentOffline", userId);
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userId, out var guid))
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == guid);
                if (user != null)
                {
                    user.IsOnline = false;
                    await _dbContext.SaveChangesAsync();

                    // 🔕 Уведомить всех, что пользователь оффлайн
                    await Clients.All.SendAsync("UserWentOffline", user.Id.ToString());
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task MarkAsDelivered(string chatId, string messageId)
        {
            var message = await _messageRepository.GetMessageByIdAsync(chatId, messageId);
            if (message == null) return;

            message.DeliveredAt ??= DateTime.UtcNow;
            await _messageRepository.UpdateMessageAsync(message);

            await Clients.Group(chatId).SendAsync("MessageDelivered", messageId, message.DeliveredAt);
        }

        public async Task MarkAsRead(string chatId, string messageId)
        {
            var message = await _messageRepository.GetMessageByIdAsync(chatId, messageId);
            if (message == null) return;

            message.IsRead = true;
            message.ReadAt ??= DateTime.UtcNow;
            await _messageRepository.UpdateMessageAsync(message);

            await Clients.Group(chatId).SendAsync("MessageRead", messageId, message.ReadAt);
        }
    }
}