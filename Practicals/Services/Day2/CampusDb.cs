using Microsoft.EntityFrameworkCore;
using static Practicals.Models.Day2;

namespace Practicals.Services.Day2
{
    public class CampusDb(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();

        protected override void OnModelCreating(ModelBuilder m)
        {
            m.Entity<Course>().Property(x => x.Title).HasMaxLength(100).IsRequired();
            m.Entity<Student>().Property(x => x.FullName).HasMaxLength(100).IsRequired();
            m.Entity<Student>().Property(x => x.Email).HasMaxLength(150).IsRequired();
            m.Entity<Student>().HasIndex(x => x.Email).IsUnique();
            m.Entity<Student>().HasOne(x => x.Course).WithMany(x => x.Students)
                .HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);
            m.Entity<Enrollment>().HasIndex(x => new { x.StudentId, x.CourseId }).IsUnique();
            m.Entity<Enrollment>().HasOne(x => x.Student).WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
            m.Entity<Enrollment>().HasOne(x => x.Course).WithMany()
                .HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);
            m.Entity<Course>().HasData(new Course { Id = 1, Title = "C#" }, new Course { Id = 2, Title = "Web API" });
            m.Entity<Student>().HasData(
                new Student { Id = 1, FullName = "Asha Patel", Email = "asha@lab.local", Year = 1, CourseId = 1, DateOfBirth = new DateTime(2003, 1, 10) },
                new Student { Id = 2, FullName = "Ravi Shah", Email = "ravi@lab.local", Year = 2, CourseId = 1, DateOfBirth = new DateTime(2002, 4, 5) },
                new Student { Id = 3, FullName = "Meera Joshi", Email = "meera@lab.local", Year = 1, CourseId = 2, DateOfBirth = new DateTime(2004, 6, 15) });
            m.Entity<Enrollment>().HasData(new Enrollment { Id = 1, StudentId = 1, CourseId = 2 });
        }

    }


}
