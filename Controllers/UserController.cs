using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TelegramWebAPI.Data;
using TelegramWebAPI.Models;
using TelegramWebAPI.Models.Requests;
using TelegramWebAPI.Services.Interfaces;

namespace TelegramWebAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var guid))
                return Unauthorized("Не удалось получить ID из токена");

            var user = await _db.Users.FindAsync(guid);
            return user == null ? NotFound("Пользователь не найден") : Ok(user);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _db.Users.FindAsync(id);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(request.Nickname))
                user.Nickname = request.Nickname;

            if (request.DateOfBirth.HasValue)
                user.DateOfBirth = request.DateOfBirth.Value;

            await _db.SaveChangesAsync();
            return Ok(user);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var guid))
                return Unauthorized("Неверный идентификатор пользователя");

            var user = await _db.Users.FindAsync(guid);
            if (user == null) return NotFound("Пользователь не найден");

            if (!string.IsNullOrWhiteSpace(request.Nickname))
                user.Nickname = request.Nickname;

            if (request.DateOfBirth.HasValue)
                user.DateOfBirth = request.DateOfBirth.Value;

            if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
                user.AvatarUrl = request.AvatarUrl;

            await _db.SaveChangesAsync();
            return Ok(user);
        }

        [Authorize]
        [HttpPost("me/avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file, [FromServices] IBlobStorageService blobService)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не выбран");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var guid))
                return Unauthorized();

            var user = await _db.Users.FindAsync(guid);
            if (user == null)
                return NotFound();

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"avatars/{user.Id}{ext}";

            using var stream = file.OpenReadStream();
            var avatarUrl = await blobService.UploadFileAsync(fileName, stream);

            user.AvatarUrl = avatarUrl;
            await _db.SaveChangesAsync();

            return Ok(new { avatarUrl });
        }

        [Authorize]
        [HttpGet("friends")]
        public async Task<IActionResult> GetFriends()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var guid))
                return Unauthorized();

            var currentUser = await _db.Users.FindAsync(guid);
            if (currentUser == null)
                return NotFound("Пользователь не найден");

            // Найти ID пользователей, с которыми есть личные чаты
            var privateChatUserIds = await _db.Chats
                .Where(c => !c.IsGroup && c.Participants.Contains(guid.ToString()))
                .SelectMany(c => c.Participants)
                .Where(pid => pid != guid.ToString())
                .Distinct()
                .ToListAsync();

            var friends = await _db.Users
                .Where(u => privateChatUserIds.Contains(u.Id.ToString()))
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
        [Authorize]
        [HttpPost("add-friend/{friendId}")]
        public async Task<IActionResult> AddFriend(Guid friendId, [FromServices] IChatService chatService)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(currentUserId, out var guid))
                return Unauthorized("Неверный ID пользователя");

            if (guid == friendId)
                return BadRequest("Нельзя добавить самого себя в друзья");

            var friend = await _db.Users.FindAsync(friendId);
            if (friend == null)
                return NotFound("Пользователь не найден");

            // Проверка: есть ли уже личный чат
            var existingChat = await chatService.GetOrCreateChatAsync(guid.ToString(), friendId.ToString());
            if (existingChat == null)
                return StatusCode(500, "Не удалось создать чат");

            return Ok(new { message = "Друг добавлен", chatId = existingChat.Id });
        }
        [Authorize]
        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetUserStatus(Guid id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Id,
                user.Nickname,
                user.AvatarUrl,
                IsOnline = user.IsOnline
            });
        }
    }
}