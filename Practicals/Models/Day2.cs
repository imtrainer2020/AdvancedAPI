namespace Practicals.Models
{
    public class Day2
    {
        public class Course
        {
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public List<Student> Students { get; set; } = new();
        }
        public class Student
        {
            public int Id { get; set; }
            public string FullName { get; set; } = "";
            public string Email { get; set; } = "";
            public DateTime DateOfBirth { get; set; }
            public string? Address { get; set; }
            public string? Phone { get; set; }
            public int Year { get; set; }
            public int CourseId { get; set; }
            public Course Course { get; set; } = null!;
            public List<Enrollment> Enrollments { get; set; } = new();
        }
        public class Enrollment
        {
            public int Id { get; set; }
            public int StudentId { get; set; }
            public Student Student { get; set; } = null!;
            public int CourseId { get; set; }
            public Course Course { get; set; } = null!;
        }
    }
}
