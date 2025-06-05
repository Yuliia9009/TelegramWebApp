using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TelegramWebAPI.Models.Chat
{

    public class Chat
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; }
        public List<string> Participants { get; set; } = new();
        public bool IsGroup { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
    }
}