using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using static Practicals.Models.Day3;

namespace Practicals.Services.Day3
{
    public sealed class LabSecurity(IOptions<JwtSettings> settings)
    {
        private readonly PasswordHasher<LabUser> _hasher = new();
        private readonly List<LabUser> _users = CreateUsers();
        private static List<LabUser> CreateUsers()
        {
            var h = new PasswordHasher<LabUser>();
            var a = new LabUser { Id = 1, Name = "admin", Role = "Admin" };
            var u = new LabUser { Id = 2, Name = "student", Role = "User" };
            a.PasswordHash = h.HashPassword(a, "Classroom-Admin-42!");
            u.PasswordHash = h.HashPassword(u, "Classroom-User-42!");
            return new() { a, u };
        }
        public (string First, string Second, bool Verified) HashTwice(string password)
        {
            var x = new LabUser();
            var one = _hasher.HashPassword(x, password);
            var two = _hasher.HashPassword(x, password);
            var ok = _hasher.VerifyHashedPassword(x, one, password) != PasswordVerificationResult.Failed
                  && _hasher.VerifyHashedPassword(x, two, password) != PasswordVerificationResult.Failed;
            return (one, two, ok);
        }
        public LoginResult? Login(LoginDto dto)
        {
            var user = _users.SingleOrDefault(x => x.Name.Equals(dto.UserName.Trim(), StringComparison.OrdinalIgnoreCase));
            if (user is null) return null;
            if (_hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password) == PasswordVerificationResult.Failed)
                return null;
            var s = settings.Value;
            var expires = DateTime.UtcNow.AddMinutes(s.ExpiryMinutes);
            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = s.Issuer,
                Audience = s.Audience,
                NotBefore = DateTime.UtcNow,
                Expires = expires,
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("sub", user.Id.ToString()),
                new Claim("name", user.Name),
                new Claim("role", user.Role)
            }),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s.SigningKey)),
                    SecurityAlgorithms.HmacSha256)
            };
            return new LoginResult(new JsonWebTokenHandler().CreateToken(descriptor), expires);
        }

    }
}
