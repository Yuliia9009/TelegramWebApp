using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TelegramWebAPI.Hubs;
using TelegramWebAPI.Settings;

namespace TelegramWebAPI.Services
{
    public class SignalRService
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly SignalRSettings _settings;

        public SignalRService(IHubContext<ChatHub> hubContext, IOptions<SignalRSettings> options)
        {
            _hubContext = hubContext;
            _settings = options.Value;

            // Console.WriteLine($"SignalR подключен к: {_settings.ConnectionString}");
        }

        // Отправить сообщение конкретному пользователю по userId (тот, что присваивается в токене)
        public Task SendToUserAsync(string userId, object message)
        {
            return _hubContext.Clients.User(userId).SendAsync("ReceiveMessage", message);
        }

        // Отправить сообщение в группу (например, чат с ChatId)
        public Task SendToGroupAsync(string groupId, object message)
        {
            return _hubContext.Clients.Group(groupId).SendAsync("ReceiveMessage", message);
        }
    }
}