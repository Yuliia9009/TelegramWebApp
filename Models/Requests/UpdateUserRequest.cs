namespace TelegramWebAPI.Models.Requests
{
    public class UpdateUserRequest
    {
        public string Nickname { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
    }
}