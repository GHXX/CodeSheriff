using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSheriff
{
    public class Database : DbContext
    {
        public Database()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite("Data Source = database.db");
        public DbSet<IgnoredUsers> IgnoredUsers { get; set; }
        public DbSet<InvaildWord> InvaildWords { get; set; }
    }
    public class IgnoredUsers
    {
        [Key]
        public ulong IgnoredUserId { get; set; }
    }

    public class InvaildWord
    {
        [Key]
        public int EntryID { get; set; }
        public string Keyword { get; set; }
        [NotMapped]
        public List<string> Reasons { get; set; }
    }
}