using System.ComponentModel.DataAnnotations;
namespace Practicals.DTOs
{
    public class Day1Dto
    {
    }

    public record StudentDto(int Id, string Name, string Email, int CourseId);

    public class CreateStudentDto
    {
        [Required, StringLength(80, MinimumLength = 2)]
        public string Name { get; set; } = "";
        [Required, EmailAddress, StringLength(120)]
        public string Email { get; set; } = "";
        [Range(1, int.MaxValue)]
        public int CourseId { get; set; }
    }
    public class UpdateStudentDto
    {
        [Required, StringLength(80, MinimumLength = 2)]
        public string Name { get; set; } = "";
        [Required, EmailAddress, StringLength(120)]
        public string Email { get; set; } = "";
        [Range(1, int.MaxValue)]
        public int CourseId { get; set; }
    }
    public class RenameStudentDto
    {
        [Required, StringLength(80, MinimumLength = 2)]
        public string Name { get; set; } = "";
    }

}
