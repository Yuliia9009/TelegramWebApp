using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using TelegramWebAPI.Settings;
using TelegramWebAPI.Services.Interfaces;

namespace TelegramWebAPI.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IOptions<AzureBlobStorageSettings> options)
        {
            var settings = options.Value;

            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new ArgumentNullException(nameof(settings.ConnectionString), "❌ Не указан ConnectionString в AzureStorage");

            if (string.IsNullOrWhiteSpace(settings.ContainerName))
                throw new ArgumentNullException(nameof(settings.ContainerName), "❌ Не указан ContainerName в AzureStorage");

            _containerClient = new BlobContainerClient(settings.ConnectionString, settings.ContainerName);
            _containerClient.CreateIfNotExists();
        }

        public async Task<string> UploadTextAsync(string fileName, string content)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            await blobClient.UploadAsync(stream, overwrite: true);
            return blobClient.Uri.ToString();
        }
        public async Task<string> UploadFileAsync(string fileName, Stream content)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(content, overwrite: true);
            return blobClient.Uri.ToString();
        }
    }
}