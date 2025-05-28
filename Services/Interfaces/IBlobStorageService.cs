using System.Threading.Tasks;

namespace TelegramWebAPI.Services.Interfaces
{
    public interface IBlobStorageService
    {
        Task<string> UploadTextAsync(string fileName, string content);
    }
}