using Microsoft.EntityFrameworkCore;
using Practicals.Models;
using static Practicals.Models.Day2;

namespace Practicals.Services.Day2
{
    public class Day2DbContext : DbContext
    {
        public Day2DbContext(DbContextOptions<Day2DbContext> options) : base(options) { }
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            //mb.Entity<Enrollment>().HasIndex(e => e.IdempotencyKey).IsUnique();
            mb.Entity<Enrollment>().HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
            mb.Entity<Student>().HasIndex(s => s.Email).IsUnique();
        }
    }
}
