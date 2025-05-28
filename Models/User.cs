using System;
using System.ComponentModel.DataAnnotations;

namespace TelegramWebAPI.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Nickname { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        public string PhoneNumber { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsOnline { get; set; } = false;
    }
}