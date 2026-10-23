using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Practicals.Models;
using Practicals.Services.Day3;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Practicals.Controllers
{
    //[EnableCors("AllowSpecificOrigins")]
    [Route("api/[controller]")]
    [ApiController]
    public class Day3Controller : ControllerBase
    {
        private readonly Day3DbContext db;
        private readonly IConfiguration config;

        public Day3Controller(Day3DbContext _db, IConfiguration configuration)
        {
            db = _db;
            config = configuration;
        }

        [AllowAnonymous, OutputCache(Duration = 20), HttpGet("simple-caching")]
        public IActionResult SimpleCaching() => Ok(new { generatedAt = DateTime.UtcNow, data = Guid.NewGuid().ToString() });

        [HttpGet("cached")]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Client)]
        public IActionResult GetCachedData()
        {
            return Ok(new { Time = DateTime.Now.ToString("HH:mm:ss") });
        }

        [HttpGet("rate-limited")]
        [SimpleRateLimit(Seconds = 5)]
        public IActionResult GetRateLimitedData()
        {
            return Ok("Success! Aapne rate limit cross nahi ki hai.");
        }

        #region Simple Authentication & Authorization

        [HttpPost("simpleauth")]
        public async Task<IActionResult> SimpleAuth([FromBody] UserLoginDto dto)
        {
            if (dto.Email == null || dto.Password == null || dto.Email.Length == 0 || dto.Password.Length == 0)
                return BadRequest("Email and Password both are required.");

            else if (dto.Email.ToLower() == "admin@admin.com" && dto.Password == "Admin@123")
            {
                // 1. Create Claims (User info & Roles)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, dto.Email),
                    new Claim(ClaimTypes.Role, "Admin") // Assigning a role
                };

                // 2. Create Identity
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // 3. Sign In (Generates the encrypted cookie)
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity));

                var response = new
                {
                    Message = "Login successful. Cookie has been set.",
                    User = new
                    {
                        Username = dto.Email,
                        Role = "Admin",
                        LoginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                };

                return Ok(response);
            }
            else
                return Unauthorized("Invalid Credentials.");
        }

        [HttpPost("simple-logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok("Logged out securely.");
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            if (User.Identity?.IsAuthenticated != true)
                return Unauthorized("Please login first.");

            string username = User.Identity.Name!;

            return Ok(new
            {
                Message = "You are authenticated!",
                Username = username
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-user")]
        public async Task<IActionResult> DeleteUser()
        {
            return Ok("Admin deleted the user.");
        }

        [AllowAnonymous]
        [HttpGet("public")]
        public async Task<IActionResult> PublicData()
        {
            return Ok("Anyone can access this endpoint.");
        }

        #endregion

        #region JWT Authentication & Authorization, RBAC and DB CRUD Operations

        // 1. Login and generate JWT
        [AllowAnonymous]
        [HttpPost("jwt-login")]
        public async Task<IActionResult> JWTLogin(UserLoginDto dto)
        {
            if (dto.Email == null || dto.Password == null || dto.Email.Length == 0 || dto.Password.Length == 0)
                return BadRequest("Email and Password both are required.");

            //else if (dto.Email.ToLower() == "admin@admin.com" && dto.Password == "Admin@123")
            //    role = "Admin";
            //else if (dto.Email.ToLower() == "student@student.com" && dto.Password == "Student@123")
            //    role = "Student";
            //else if (dto.Email.ToLower() == "user@user.com" && dto.Password == "User@123")
            //    role = "User";
            //else
            //    return Unauthorized("Invalid username or password");

            var user = await db.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower()
                && u.Password == dto.Password);
            if (user == null)
                return Unauthorized("Invalid username or password");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, dto.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler()
                    .WriteToken(token)
            });
        }

        // 2. Any authenticated user can access
        [Authorize]
        [HttpGet("logged-user-profile")]
        public async Task<IActionResult> LoggedInUserProfile()
        {
            //return Ok(new
            //{
            //    Username = User.Identity!.Name,
            //    Role = User.FindFirst(ClaimTypes.Role)?.Value,
            //    Message = "You are authenticated!"
            //});

            //var user = await db.Users.AsNoTracking()
            //    .FirstOrDefaultAsync(u => u.Email.ToLower().Equals(User.Identity!.Name.ToLower(), StringComparison.CurrentCultureIgnoreCase));
            var user = await db.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.Email.ToLower() == User.Identity!.Name.ToLower());
            return Ok(user);

        }

        // 3. Only Admin and User can access
        [Authorize(Roles = "Admin, User")]
        [HttpDelete("jwt-delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await db.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found");

            db.Users.Remove(user);
            await db.SaveChangesAsync();
            return Ok($"User deleted");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("exam-details")]
        public async Task<IActionResult> AddExamDetails([FromBody] AddExamDto dto)
        {
            var exam = new ExamDetails
            {
                ExamName = dto.ExamName,
                ExamDate = dto.ExamDate,
                ExamLocation = dto.ExamLocation
            };

            await db.ExamDetails.AddAsync(exam);
            await db.SaveChangesAsync();

            return Ok(new { Message = "Exam added successfully", ExamId = exam.Id });
        }

        [Authorize(Roles = "Student")]
        [HttpGet("my-results")]
        public async Task<IActionResult> GetMyResults()
        {
            var user = await db.Users.
                FirstOrDefaultAsync(u => u.Email.ToLower() == User.Identity!.Name.ToLower());

            if (user == null) return Unauthorized();

            // LINQ Join for particular student's results
            var myResults = await (from er in db.ExamResults
                                   join e in db.ExamDetails on er.ExamId equals e.Id
                                   where er.UserId == user.Id
                                   select new MyResultDto(e.ExamName, e.ExamDate, er.Score))
                           .ToListAsync();

            return Ok(myResults);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpGet("all-results")]
        public async Task<IActionResult> GetAllResults()
        {
            // All students ke results with their names and exam details
            var allResults = from er in db.ExamResults
                             join u in db.Users on er.UserId equals u.Id
                             join e in db.ExamDetails on er.ExamId equals e.Id
                             select new StudentResultDto(u.Name, e.ExamName, er.Score);

            return Ok(allResults.ToList());
        }

        #endregion

    }


    public class SimpleRateLimitAttribute : ActionFilterAttribute
    {
        // Static dictionary IP addresses aur unki last request time store karne ke liye
        private static ConcurrentDictionary<string, DateTime> _ipHistory = new();

        public int Seconds { get; set; } = 5; // Default 5 seconds limit

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // User ka IP address get karein
            var clientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";

            // Check karein kya user pehle request kar chuka hai
            if (_ipHistory.TryGetValue(clientIp, out var lastRequestTime))
            {
                // Agar pichli request ko enough time nahi hua hai
                if ((DateTime.Now - lastRequestTime).TotalSeconds < Seconds)
                {
                    // Request ko yahin block kar dein (429 Too Many Requests)
                    context.Result = new ObjectResult("Too many requests! Please wait for a few seconds.")
                    {
                        StatusCode = StatusCodes.Status429TooManyRequests
                    };
                    return;
                }
            }

            // IP ka time update karein aur aage badhne dein
            _ipHistory[clientIp] = DateTime.Now;

            base.OnActionExecuting(context);
        }
    }
}
