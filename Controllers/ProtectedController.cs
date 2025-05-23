using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            return Ok(new
            {
                message = "✅ Доступ разрешён",
                user = User.Identity?.Name
            });
        }
    }
}