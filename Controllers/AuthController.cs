using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TelegramWebAPI.Data;
using TelegramWebAPI.Models;
using TelegramWebAPI.Models.Requests;
using TelegramWebAPI.Services.Interfaces;

namespace TelegramWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static readonly Dictionary<string, string> otpStore = new(); // временное хранилище OTP
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
            otpStore[request.PhoneNumber] = code;

            Console.WriteLine($"[DEBUG] OTP for {request.PhoneNumber}: {code}");

            return Ok(new { message = "Code sent successfully (mock)." });
        }

            [HttpPost("verify-code")]
            public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
            {
                Console.WriteLine($"📲 Phone: {request.PhoneNumber}, Code: {request.Code}");

                if (!otpStore.TryGetValue(request.PhoneNumber, out var code) || code != request.Code)
                    return Unauthorized("Invalid code.");

                var user = _context.Users.FirstOrDefault(u => u.PhoneNumber == request.PhoneNumber);

                if (user == null)
                {
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        PhoneNumber = request.PhoneNumber,
                        Nickname = $"User_{Guid.NewGuid().ToString()[..6]}",
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

        [HttpPost("complete-registration")]
        public async Task<IActionResult> CompleteRegistration([FromBody] CompleteRegistrationRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.PhoneNumber == request.PhoneNumber);
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