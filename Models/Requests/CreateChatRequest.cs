namespace TelegramWebAPI.Models.Requests
{
    public class CreateChatRequest
    {
        public string Name { get; set; }
        public List<string> Participants { get; set; }
        public bool IsGroup { get; set; }
    }
}