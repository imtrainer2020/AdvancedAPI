using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Practicals.Models
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
    public class Day2
    {
        // Added 'public' here
        public class Student
        {
            public int Id { get; set; }
            [Required, MinLength(2), MaxLength(50)]
            public string Name { get; set; } = "";
            [Required, EmailAddress]
            public string Email { get; set; } = "";
            public string? KeycloakUser { get; set; }
            public string InternalNotes { get; set; } = "staff only";
            public List<Enrollment> Enrollments { get; set; } = new();
        }

        // Added 'public' here
        public class Course
        {
            public int Id { get; set; }
            [Required, MinLength(3), MaxLength(80)]
            public string Title { get; set; } = "";
            [Range(0, 100000)]
            public decimal Fee { get; set; }
            public int SeatsAvailable { get; set; }
            [Timestamp]
            public byte[] RowVersion { get; set; } = [];
        }

        // Added 'public' here
        public class Enrollment
        {
            public int Id { get; set; }
            public int StudentId { get; set; }
            public int CourseId { get; set; }

            //public string IdempotencyKey { get; set; } = "";
            public Student? Student { get; set; }
            public DateTime AtUtc { get; set; }
        }

        // Added 'public' here
        public class AuditLog
        {
            public long Id { get; set; }
            public string Who { get; set; } = "";
            public string Did { get; set; } = "";
            public DateTime AtUtc { get; set; }
        }

        // Added 'public' to DTOs so your Controller can use them safely later
        public class CourseDto
        {
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public decimal Fee { get; set; }
            public int SeatsAvailable { get; set; }
            public string RowVersion { get; set; } = "";
        }

        public class SaveCourse
        {
            [Required, MinLength(3), MaxLength(80)]
            public string Title { get; set; } = "";
            [Range(0, 100000)]
            public decimal Fee { get; set; }
            [Range(0, 500)]
            public int SeatsAvailable { get; set; }
        }

        public class SaveStudent
        {
            [Required, MinLength(2), MaxLength(50)]
            public string Name { get; set; } = "";
            [Required, EmailAddress]
            public string Email { get; set; } = "";
        }

        public class StudentDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Email { get; set; } = "";
        }

        public class EnrollRequest
        {
            [Range(1, int.MaxValue)]
            public int StudentId { get; set; }
            [Range(1, int.MaxValue)]
            public int CourseId { get; set; }
        }
    }
}
