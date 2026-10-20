using System.ComponentModel.DataAnnotations;

namespace Practicals.DTOs
{
    public class Day2Dto
    {
        public record StudentsDto(int Id, string FullName, string Email, string? Phone,
        DateTime DateOfBirth, int Year, int CourseId, string CourseTitle);
    }
    public sealed class NotFutureAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value) => value is null ||
            value is DateTime d && d.Date <= DateTime.Today;
    }
    public class StudentWrite
    {
        [Required, StringLength(100, MinimumLength = 3)]
        public string FullName { get; set; } = "";
        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = "";
        [Required, NotFuture(ErrorMessage = "Birth date cannot be in the future.")]
        public DateTime? DateOfBirth { get; set; }
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Use a 10-digit mobile starting6–9.")]
        public string? Phone { get; set; }
        [Range(1, 5)]
        public int Year { get; set; }
        [Range(1, int.MaxValue)]
        public int CourseId { get; set; }
    }
    public class StudentUpdate : StudentWrite { }

    public record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount)
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

}
