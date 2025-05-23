using Azure.Storage.Blobs;

namespace TelegramWebAPI.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public async Task<string> UploadTextAsync(string fileName, string content)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            await blobClient.UploadAsync(stream, overwrite: true);
            return blobClient.Uri.ToString();
        }

        public BlobStorageService(IConfiguration config)
        {
            var connection = config["Azure:AzureStorage:ConnectionString"];
            var container = config["Azure:AzureStorage:ContainerName"];

            if (string.IsNullOrWhiteSpace(connection))
                throw new ArgumentNullException(nameof(connection), "❌ Не указан ConnectionString в AzureStorage");

            if (string.IsNullOrWhiteSpace(container))
                throw new ArgumentNullException(nameof(container), "❌ Не указан ContainerName в AzureStorage");

            _containerClient = new BlobContainerClient(connection, container);
            _containerClient.CreateIfNotExists();
        }
    }
}