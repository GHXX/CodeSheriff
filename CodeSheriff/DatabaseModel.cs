using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CodeSheriff.DatabaseModel
{
    public class Model : DbContext
    {
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
        public string[] Reasons { get; set; }
    }
}
