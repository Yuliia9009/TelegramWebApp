namespace TelegramWebAPI.Models.Requests
{
    public class SendMessageRequest
    {
        public string ChatId { get; set; }
        public string Text { get; set; }
    }
}