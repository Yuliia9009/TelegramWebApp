using Microsoft.AspNetCore.Mvc;
using TelegramWebAPI.Services;

namespace TelegramWebAPI.Controllers
{
    [ApiController]
    [Route("api/system-test")]
    public class SystemTestController : ControllerBase
    {
        private readonly CosmosDbService _cosmosService;

        public SystemTestController(CosmosDbService cosmosService)
        {
            _cosmosService = cosmosService;
        }

        [HttpGet("ping")]
        public async Task<IActionResult> Ping()
        {
            var savedMessage = await _cosmosService.SaveTestMessageAsync();

            return Ok(new
            {
                status = "✅ Система работает",
                savedToCosmosDb = savedMessage
            });
        }
    }
}