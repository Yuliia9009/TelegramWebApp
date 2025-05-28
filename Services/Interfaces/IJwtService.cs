using TelegramWebAPI.Models;

namespace TelegramWebAPI.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}