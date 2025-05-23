using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelegramWebAPI.Data;
using TelegramWebAPI.Models;
using TelegramWebAPI.Models.Requests;

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
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var b2cUserId = User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(b2cUserId))
                return Unauthorized("❌ Не удалось получить идентификатор пользователя из токена");

            var existing = await _db.Users.FirstOrDefaultAsync(u => u.B2CUserId == b2cUserId);
            if (existing != null)
                return Conflict("👤 Пользователь уже зарегистрирован");

            var user = new User
            {
                Id = Guid.NewGuid(),
                B2CUserId = b2cUserId,
                Nickname = request.Nickname,
                DateOfBirth = request.DateOfBirth,
                PhoneNumber = request.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(user);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var b2cUserId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(b2cUserId))
                return Unauthorized();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.B2CUserId == b2cUserId);
            return user == null ? NotFound() : Ok(user);
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

            user.Nickname = request.Nickname;
            user.DateOfBirth = request.DateOfBirth;
            user.PhoneNumber = request.PhoneNumber;

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
    }
}