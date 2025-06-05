// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.SignalR;
// using TelegramWebAPI.Hubs;

// namespace TelegramWebAPI.Controllers
// {
//     [ApiController]
//     [Route("api/test-signalr")]
//     public class SignalTestController : ControllerBase
//     {
//         private readonly IHubContext<ChatHub> _hubContext;

//         public SignalTestController(IHubContext<ChatHub> hubContext)
//         {
//             _hubContext = hubContext;
//         }

//         [HttpGet("send")]
//         public async Task<IActionResult> Send()
//         {
//             await _hubContext.Clients.All.SendAsync("ReceiveMessage", "System", "📡 SignalR работает!");
//             return Ok("Сообщение отправлено через SignalR");
//         }
//     }
// }