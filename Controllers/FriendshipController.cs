using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TelegramWebAPI.Data;
using TelegramWebAPI.Models;
using TelegramWebAPI.Models.Requests;
using TelegramWebAPI.Models.Chat;
using TelegramWebAPI.Services.Interfaces;
using System.Security.Claims;

namespace TelegramWebAPI.Controllers
{
    [ApiController]
    [Route("api/friends")]
    public class FriendshipController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IChatService _chatService;

        public FriendshipController(ApplicationDbContext db, IChatService chatService)
        {
            _db = db;
            _chatService = chatService;
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddFriend([FromBody] AddFriendRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                return BadRequest("Номер телефона обязателен");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var myId))
                return Unauthorized();

            var friend = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber.Trim());
            if (friend == null || friend.Id == myId)
                return NotFound("Пользователь не найден");

            var alreadyFriends = await _db.Friendships.AnyAsync(f =>
                f.UserId == myId && f.FriendId == friend.Id);

            if (alreadyFriends)
                return BadRequest("Пользователь уже в списке друзей");

            _db.Friendships.Add(new Friendship { UserId = myId, FriendId = friend.Id });
            await _db.SaveChangesAsync();

            var existingChat = await _chatService.GetOrCreatePrivateChatAsync(myId.ToString(), friend.Id.ToString());
            if (existingChat == null)
            {
                var newChat = new Chat
                {
                    IsGroup = false,
                    Participants = new List<string> { myId.ToString(), friend.Id.ToString() },
                    CreatedBy = myId.ToString(),
                    Name = $"Чат с {friend.Nickname}"
                };

                await _chatService.CreateChatAsync(newChat);
            }

            return Ok(new { message = "Друг добавлен", friendId = friend.Id });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMyFriends()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var myId))
                return Unauthorized();

            var friendIds = await _db.Friendships
                .Where(f => f.UserId == myId)
                .Select(f => f.FriendId)
                .ToListAsync();

            var friends = await _db.Users
                .Where(u => friendIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.Nickname,
                    u.PhoneNumber,
                    u.AvatarUrl,
                    u.IsOnline
                })
                .ToListAsync();

            return Ok(friends);
        }
    }
}