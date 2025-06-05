using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TelegramWebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/protected")]
    public class ProtectedController : ControllerBase
    {
        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var nickname = User.FindFirst(ClaimTypes.Name)?.Value;
            var phone = User.FindFirst("phone")?.Value;

            return Ok(new
            {
                message = "✅ Доступ разрешён",
                user = new
                {
                    id = userId,
                    nickname,
                    phone
                }
            });
        }
    }
}