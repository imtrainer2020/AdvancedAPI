using Microsoft.EntityFrameworkCore;
using Practicals.Models;

namespace Practicals.Services.Day3
{
    public class Day3DbContext : DbContext
    {
        public Day3DbContext(DbContextOptions<Day3DbContext> options)
            : base(options)
        { }

        public DbSet<Users> Users => Set<Users>();
        public DbSet<ExamDetails> ExamDetails => Set<ExamDetails>();
        public DbSet<ExamResult> ExamResults => Set<ExamResult>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Users>().HasIndex(u => u.Email).IsUnique();
            mb.Entity<ExamResult>().HasIndex(er => new { er.UserId, er.ExamId }).IsUnique();
        }
    }
}
