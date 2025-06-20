using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TelegramWebAPI.Data;
using TelegramWebAPI.Models;
using TelegramWebAPI.Models.Requests;
using TelegramWebAPI.Services.Interfaces;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace TelegramWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, string> otpStore = new();
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtService _jwtService;

        public AuthController(
            ApplicationDbContext context,
            IPasswordHasher<User> passwordHasher,
            IJwtService jwtService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        [HttpPost("send-code")]
        public IActionResult SendCode([FromBody] SendCodeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                return BadRequest("Phone number is required.");

            var code = new Random().Next(100000, 999999).ToString();
            otpStore[request.PhoneNumber.ToLower()] = code;

            Console.WriteLine($"[DEBUG] OTP for {request.PhoneNumber}: {code}");

            return Ok(new { message = "Code sent successfully (mock)." });
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Неверный формат запроса.");

                var normalizedPhone = request.PhoneNumber.ToLower();

                if (!otpStore.TryGetValue(normalizedPhone, out var storedCode))
                    return Unauthorized("Код не найден или срок действия истёк.");

                if (storedCode != request.Code)
                    return Unauthorized("Неверный код.");

                otpStore.TryRemove(normalizedPhone, out _); // Удалить код после использования

                var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);

                if (user == null)
                {
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        PhoneNumber = request.PhoneNumber,
                        Nickname = $"User_{Guid.NewGuid():N}"[..6],
                        DateOfBirth = DateTime.UtcNow,
                        PasswordHash = _passwordHasher.HashPassword(null, Guid.NewGuid().ToString())
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                var token = _jwtService.GenerateToken(user);

                return Ok(new
                {
                    token,
                    user = new
                    {
                        user.Id,
                        user.Nickname,
                        user.PhoneNumber
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка при верификации: {ex.Message}");
                return StatusCode(500, "Ошибка сервера. Попробуйте позже.");
            }
        }

        [HttpPost("complete-registration")]
        public async Task<IActionResult> CompleteRegistration([FromBody] CompleteRegistrationRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
            if (user == null)
                return BadRequest("User not found.");

            user.Nickname = request.Nickname;
            user.DateOfBirth = request.DateOfBirth;
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Nickname,
                    user.PhoneNumber
                }
            });
        }
    }
}