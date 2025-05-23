namespace TelegramWebAPI.Models
{
    public class TestMessage
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string Text { get; set; } = "Test message from API";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string chatId { get; set; } = "default"; 
    }
}