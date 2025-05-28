using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TelegramWebAPI.Models.Requests
{
    public class VerifyCodeRequest
    {
        [Required]
        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }

        [Required]
        [JsonPropertyName("code")]
        public string Code { get; set; }
    }
}