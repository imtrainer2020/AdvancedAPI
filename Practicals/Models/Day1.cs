namespace Practicals.Models
{
    public class Day1
    {

    }

    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public int CourseId { get; set; }
        public string InternalNote { get; set; } = "Staff only";
    }
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
    }
    public class Unit
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
    public class Personnel
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string Name { get; set; } = "";
    }
    public static class Day1Store
    {
        public static readonly List<Student> Students = new()
        {
            new Student { Id = 1, Name = "Asha", Email = "asha@lab.local", CourseId = 1 },
            new Student { Id = 2, Name = "Ravi", Email = "ravi@lab.local", CourseId = 2 }
        };
        public static readonly List<Course> Courses = new()
        {
            new Course { Id = 1, Title = "C# Basics" },
            new Course { Id = 2, Title = "Web API" }
        };
        public static readonly List<Unit> Units = new()
        {
            new Unit { Id = 1, Name = "Training" },
            new Unit { Id = 2, Name = "Support" }
        };
        public static readonly List<Personnel> People = new()
        {
            new Personnel { Id = 1, UnitId = 1, Name = "Ishan" },
            new Personnel { Id = 2, UnitId = 1, Name = "Meera" }
        };
        public static int NextStudentId = 3;
        public static int NextCourseId = 3;
    }



}
