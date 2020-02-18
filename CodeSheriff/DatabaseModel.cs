using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading;

namespace CodeSheriff.DatabaseModel
{
    public class Model : DbContext
    {
        public SemaphoreSlim DbSemaphore { get; set; } = new SemaphoreSlim(1, 1);
        public DbSet<IgnoredUser> IgnoredUsers { get; private set; }
        public DbSet<FlaggedWord> FlaggedWords { get; private set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source = sheriff.db");
    }

    public class IgnoredUser
    {
        [Key]
        public uint PrimaryKey { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
    }

    public class FlaggedWord
    {
        [Key]
        public uint PrimaryKey { get; set; }
        public ulong GuildId { get; set; }
        public string Word { get; set; }
        public string Reasons { get; set; }
    }
}
