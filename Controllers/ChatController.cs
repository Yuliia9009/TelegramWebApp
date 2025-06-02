using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelegramWebAPI.Models.Chat;
using TelegramWebAPI.Models.Requests;
using TelegramWebAPI.Services;
using TelegramWebAPI.Services.Interfaces;

namespace TelegramWebAPI.Controllers
{
    [ApiController]
    [Route("api/chats")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
        {
            var chat = new Chat
            {
                Name = request.Name,
                Participants = request.Participants,
                IsGroup = request.IsGroup
            };

            var created = await _chatService.CreateChatAsync(chat);
            return Ok(created);
        }

        [Authorize]
        [HttpPost("{chatId}/messages")]
        public async Task<IActionResult> SendMessage(string chatId, [FromBody] SendMessageRequest request)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId == null) return Unauthorized();

            var message = new Message
            {
                ChatId = chatId,
                SenderId = userId,
                Text = request.Text
            };

            var sent = await _chatService.SendMessageAsync(message);
            return Ok(sent);
        }

        [Authorize]
        [HttpGet("{chatId}/messages")]
        public async Task<IActionResult> GetMessages(string chatId)
        {
            var messages = await _chatService.GetMessagesAsync(chatId);
            return Ok(messages);
        }
        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyChats()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var chats = await _chatService.GetChatsForUserAsync(userId);
            return Ok(chats);
        }
        [HttpPut("edit-message/{messageId}")]
        public async Task<IActionResult> EditMessage(string messageId, [FromBody] string newText)
        {
            var updatedMessage = await _chatService.UpdateMessageAsync(messageId, newText);
            return Ok(updatedMessage);
        }
        [Authorize]
        [HttpPost("private")]
        public async Task<IActionResult> CreatePrivateChat([FromBody] CreatePrivateChatRequest request)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId == null) return Unauthorized();

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
    }
}