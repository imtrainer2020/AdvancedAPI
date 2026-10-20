using System.ComponentModel.DataAnnotations;

namespace Practicals.Models
{
    public class Day3
    {
        public class JwtSettings
        {
            [Required] 
            public string Issuer { get; set; } = "CampusLab";
            [Required] 
            public string Audience { get; set; } = "Students";
            [Required, MinLength(32)] 
            public string SigningKey { get; set; } = "";
            [Range(1, 60)] 
            public int ExpiryMinutes { get; set; } = 15;
        }
        public class LabUser
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string PasswordHash { get; set; } = "";
            public string Role { get; set; } = "User";
        }
        public class LoginDto
        {
            [Required] 
            public string UserName { get; set; } = "";
            [Required] 
            public string Password { get; set; } = "";
        }
        public record LoginResult(string AccessToken, DateTime ExpiresAtUtc);

    }
}
