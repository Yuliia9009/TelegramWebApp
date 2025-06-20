using Microsoft.EntityFrameworkCore;
using TelegramWebAPI.Models;
using TelegramWebAPI.Models.Chat;

namespace TelegramWebAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Chat> Chats { get; set; }
        
    }
}