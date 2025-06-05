using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelegramWebAPI.Models.Chat;
using TelegramWebAPI.Models.Requests;
using TelegramWebAPI.Services.Interfaces;
using System.Security.Claims;

namespace TelegramWebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/chats")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
        {
            if (request.Participants == null || !request.Participants.Any())
                return BadRequest("Список участников не может быть пустым.");

            var chat = new Chat
            {
                Name = request.Name,
                Participants = request.Participants,
                IsGroup = request.IsGroup
            };

            var created = await _chatService.CreateChatAsync(chat);
            return Ok(created);
        }

        [HttpPost("{chatId}/messages")]
        public async Task<IActionResult> SendMessage(string chatId, [FromBody] SendMessageRequest request)
        {
            Console.WriteLine($"👉 Запрос на сообщение в чат: {chatId}");

            if (string.IsNullOrWhiteSpace(chatId))
                return BadRequest("chatId обязателен.");

            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Текст сообщения не может быть пустым.");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            Console.WriteLine($"📝 Отправитель: {userId}, текст: {request.Text}");

            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                ChatId = chatId,
                SenderId = userId,
                Text = request.Text,
                SentAt = DateTime.UtcNow
            };

            // ❗ Только одно сохранение — через сервис
            var sent = await _chatService.SendMessageAsync(message);

            return Ok(sent);
        }

        [HttpGet("{chatId}/messages")]
        public async Task<IActionResult> GetMessages(string chatId)
        {
            Console.WriteLine($"📥 Получение сообщений для чата: {chatId}");

            if (string.IsNullOrWhiteSpace(chatId))
                return BadRequest("chatId обязателен.");

            var messages = await _chatService.GetMessagesAsync(chatId);
            return Ok(messages.OrderBy(m => m.SentAt));
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyChats()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            Console.WriteLine($"📥 Получение чатов для пользователя: {userId}");

            var chats = await _chatService.GetChatsForUserAsync(userId);
            return Ok(chats);
        }

        [HttpPut("edit-message/{chatId}/{messageId}")]
        public async Task<IActionResult> EditMessage(string chatId, string messageId, [FromBody] string newText)
        {
            if (string.IsNullOrWhiteSpace(newText))
                return BadRequest("Новый текст сообщения не может быть пустым.");

            var updatedMessage = await _chatService.UpdateMessageAsync(chatId, messageId, newText);
            return Ok(updatedMessage);
        }

        [HttpPost("private")]
        public async Task<IActionResult> CreatePrivateChat([FromBody] CreatePrivateChatRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            Console.WriteLine($"🔐 Создание личного чата: {userId} ↔ {request.ParticipantId}");

            var existingChat = await _chatService.GetOrCreatePrivateChatAsync(userId, request.ParticipantId);
            if (existingChat != null)
                return Ok(existingChat);

            var chat = new Chat
            {
                IsGroup = false,
                Participants = new List<string> { userId, request.ParticipantId },
                Name = "Личный чат",
                CreatedBy = userId
            };

            var created = await _chatService.CreateChatAsync(chat);
            return Ok(created);
        }

        [HttpGet("find-or-create/{userId}")]
        public async Task<IActionResult> FindOrCreateChat(string userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            Console.WriteLine($"🔎 Поиск/создание чата между {currentUserId} и {userId}");

            var chat = await _chatService.GetOrCreateChatAsync(currentUserId, userId);
            return Ok(chat);
        }

        // Изменён маршрут, чтобы избежать конфликтов
        [HttpGet("by-id/{id}")]
        public async Task<IActionResult> GetChatById(string id)
        {
            var chat = await _chatService.GetChatByIdAsync(id);
            if (chat == null)
                return NotFound();

            return Ok(chat);
        }
    }
}