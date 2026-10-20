using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Practicals.Services.Day3;
using static Practicals.Models.Day3;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.OutputCaching;

namespace Practicals.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Day3Controller : ControllerBase
    {
        #region "3.2 Password hashing, JWT login and [Authorize]"

        // Same password, hashed twice: different salts, both verify. Never expose real hashes.
        [AllowAnonymous]
        [HttpGet("demos/hash")]
        public IActionResult HashDemo([FromServices] LabSecurity security, [FromServices] IHostEnvironment env)
        {
            if (!env.IsDevelopment()) return NotFound();
            var r = security.HashTwice("Demo-Password-1!");
            return Ok(new { r.First, r.Second, different = r.First != r.Second, r.Verified });
        }

        [AllowAnonymous]
        [HttpPost("~/api/auth/login")]
        public ActionResult<LoginResult> Login(LoginDto dto, [FromServices] LabSecurity security)
        {
            var result = security.Login(dto);
            if (result is not null)
                return Ok(result);
            Response.Headers["WWW-Authenticate"] = "Bearer";
            return Unauthorized(new ProblemDetails { Status = 401, Title = "Invalid username or password." });
        }

        [Authorize]  // any signed-in user
        [HttpGet("me")]
        public IActionResult Me() => Ok(new
        {
            id = User.FindFirst("sub")?.Value,
            name = User.Identity?.Name,
            roles = User.FindAll("role").Select(c => c.Value)
        });

        #endregion

        #region "3.3 — Roles and policies"

        [Authorize(Roles = "Admin")]
        [HttpDelete("items/{id:int}")]
        public IActionResult DeleteItem(int id) => NoContent();

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("admin-report")]
        public IActionResult AdminReport() => Ok(new { message = "AdminOnly policy passed" });

        #endregion

        #region "3.6 — Versions, rate limiting, output cache and health checks"

        [AllowAnonymous, HttpGet("~/api/v1/catalog")]
        public IActionResult CatalogV1() => Ok(new { title = "Web API" });

        [AllowAnonymous, HttpGet("~/api/v2/catalog")]
        public IActionResult CatalogV2() => Ok(new { title = "Web API", credits = 5 });

        [AllowAnonymous, EnableRateLimiting("LabLimit"), HttpGet("rate-limiting")]
        public IActionResult Limited() => Ok(new { message = "Inside the request budget" });

        [AllowAnonymous, OutputCache(Duration = 20), HttpGet("catalog-caching")]
        public IActionResult CatalogStamp() => Ok(new { generatedAt = DateTime.UtcNow, stamp = Guid.NewGuid().ToString() });

        #endregion

        #region "3.7 Testing APIs"

        private readonly IHttpClientFactory _clientFactory;
        private readonly IWebHostEnvironment _env;

        public Day3Controller(IHttpClientFactory clientFactory, IWebHostEnvironment env)
        {
            _clientFactory = clientFactory;
            _env = env;
        }

        // 1. Plain Call (Without Polly -> Direct Fail)
        [HttpGet("plain-call")]
        public async Task<IActionResult> PlainCall()
        {
            if (!_env.IsDevelopment())
            {
                return Forbid("Development environment only.");
            }

            var client = _clientFactory.CreateClient("PlainClient");
            var response = await client.GetAsync("/flaky");

            var content = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, new
            {
                Status = (int)response.StatusCode,
                Result = content
            });
        }

        // 2. Polly Call (Retry and 3rd attempt success)
        [HttpGet("polly-retry")]
        public async Task<IActionResult> PollyRetry()
        {
            if (!_env.IsDevelopment())
            {
                return Forbid("Development environment only.");
            }

            var client = _clientFactory.CreateClient("PollyClient");

            try
            {
                var response = await client.GetAsync("/flaky");
                var content = await response.Content.ReadAsStringAsync();

                return StatusCode((int)response.StatusCode, new
                {
                    Status = (int)response.StatusCode,
                    Result = content,
                    Note = "Successfully recovered via Polly Retry!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // 3. Polly Timeout Call (Slow endpoint)
        [HttpGet("polly-timeout")]
        public async Task<IActionResult> PollyTimeout()
        {
            if (!_env.IsDevelopment())
            {
                return Forbid("Development environment only.");
            }

            var client = _clientFactory.CreateClient("PollyClient");

            try
            {
                // WireMock slow endpoint 10s wait karwayega, par Polly will cancel in 3s
                var response = await client.GetAsync("/slow");
                return Ok(await response.Content.ReadAsStringAsync());
            }
            catch (Polly.Timeout.TimeoutRejectedException)
            {
                return StatusCode(504, new { Error = "Polly Timeout: Request cancelled because server took more than 3 seconds." });
            }
        }

        #endregion

    }
}
