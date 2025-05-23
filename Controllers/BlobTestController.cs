using Microsoft.AspNetCore.Mvc;
using TelegramWebAPI.Services;

namespace TelegramWebAPI.Controllers
{
    [ApiController]
    [Route("api/test-blob")]
    public class BlobTestController : ControllerBase
    {
        private readonly BlobStorageService _blobService;

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