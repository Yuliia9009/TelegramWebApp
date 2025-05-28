using Microsoft.AspNetCore.Mvc;
using TelegramWebAPI.Services;
using TelegramWebAPI.Services.Interfaces;

namespace TelegramWebAPI.Controllers
{
    [ApiController]
    [Route("api/test-blob")]
    public class BlobTestController : ControllerBase
    {
        private readonly IBlobStorageService _blobService;

        public BlobTestController(BlobStorageService blobService)
        {
            _blobService = blobService;
        }

        [HttpGet("upload")]
        public async Task<IActionResult> Upload()
        {
            var url = await _blobService.UploadTextAsync("test-file.txt", "Тестовое содержимое");
            return Ok(new { status = "✅ Файл загружен", url });
        }
    }
}