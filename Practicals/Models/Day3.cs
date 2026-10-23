using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Practicals.Models
{
    public class Day3
    {

    }

    #region Entities

    public class Users
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string Role { get; set; } = "Guest";//Admin, User, Student, Guest
    }

    public class ExamDetails
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string ExamName { get; set; } = string.Empty;
        [Required]
        public DateTime ExamDate { get; set; } = DateTime.UtcNow.AddDays(30);
        [Required]
        public string ExamLocation { get; set; } = string.Empty;
    }

    public class ExamResult
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int ExamId { get; set; }
        [Required, Range(0, 100)]
        public decimal Score { get; set; }
    }

    #endregion

    #region DTOs

    public class UserLoginDto
    {
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }

    public record GetExamScoreDto
    {
        public string StudentName { get; init; } = string.Empty;
        public string ExamName { get; init; } = string.Empty;
        public DateTime ExamDate { get; init; }
        public decimal Score { get; init; } = 0;
    }

    public record UserProfileDto(int Id, string Name, string Email, string Role);

    public record AddExamDto(string ExamName, DateTime ExamDate, string ExamLocation);

    public record MyResultDto(string ExamName, DateTime ExamDate, decimal Score);

    public record StudentResultDto(string StudentName, string ExamName, decimal Score);

    #endregion
}
