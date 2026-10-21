using System.ComponentModel.DataAnnotations;

namespace Practicals.Models
{
    public class Day1
    {
        public class Product
        {
            public int Id { get; set; }
            [Required, MinLength(3), MaxLength(50)]
            public string Name { get; set; } = "";
            [Range(0.01, 100000)]
            public decimal Price { get; set; }
            public string Category { get; set; } = "General";
            public int Stock { get; set; }
            public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        }

        // Used ONLY by PATCH: every field optional; we merge what is sent.
        public class UpdateProduct
        {
            [MinLength(3), MaxLength(50)]
            public string? Name { get; set; }
            [Range(0.01, 100000)]
            public decimal? Price { get; set; }
            public string? Category { get; set; }
            public int? Stock { get; set; }
        }

        public class Review
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            [Range(1, 5)]
            public int Rating { get; set; }
            public string Comment { get; set; } = "";
        }

        #region "Model Binding & DTO"

        public class Student            // Entity: what we store
        {
            public int Id { get; set; }
            public string FullName { get; set; } = "";
            public string Email { get; set; } = "";
            public int CourseId { get; set; }
            public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
            public string InternalNotes { get; set; } = "";   // must never leave the server
        }

        public record CreateStudentDto                 // Input for POST
        {
            [Required, StringLength(100, MinimumLength = 2)]
            public string FullName { get; init; } = "";
            [Required, EmailAddress]
            public string Email { get; init; } = "";
            [Range(1, int.MaxValue)]
            public int CourseId { get; init; }
        }

        public record UpdateStudentDto                 // Input for PUT (full replace)
        {
            [Required, StringLength(100, MinimumLength = 2)]
            public string FullName { get; init; } = "";
            [Required, EmailAddress]
            public string Email { get; init; } = "";
            [Range(1, int.MaxValue)]
            public int CourseId { get; init; }
        }

        public record PatchStudentDto                  // Input for PATCH (all optional)
        {
            [StringLength(100, MinimumLength = 2)]
            public string? FullName { get; init; }
            [EmailAddress]
            public string? Email { get; init; }
        }

        public record StudentDto(int Id, string FullName, string Email, int CourseId);   // Output


        #endregion

    }
}