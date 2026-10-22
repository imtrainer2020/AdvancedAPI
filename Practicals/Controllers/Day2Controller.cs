using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Crucial for AnyAsync() to work!
using Practicals.Models;
using static Practicals.Models.Day2;
// Note: Ensure DILifespan exists in your project, or comment this line out if it throws an error
using static Practicals.Services.Day2.DILifespans;

namespace Practicals.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Day2Controller : ControllerBase
    {
        //Day2DbfirstContext db = new Day2DbfirstContext();
        private readonly Practicals.Services.Day2.Day2DbContext _db;

        public Day2Controller(Practicals.Services.Day2.Day2DbContext db)
        {
            this._db = db;
        }

        [HttpGet("di-lifetimes")]
        public IActionResult Lifetimes(
            [FromServices] TransientValue t1, [FromServices] TransientValue t2,
            [FromServices] ScopedValue s1, [FromServices] ScopedValue s2,
            [FromServices] SingletonValue a1, [FromServices] SingletonValue a2)
        {
            return Ok(new
            {
                transient = new[] { t1.Id, t2.Id },
                scoped = new[] { s1.Id, s2.Id },
                singleton = new[] { a1.Id, a2.Id }
            });
        }

        [HttpPost("seed")]
        public async Task<IActionResult> DataEntry()
        {
            if (await _db.Students.AnyAsync())
                return Ok("Already seeded");
            _db.Students.AddRange(
                new Student { Name = "Trainer Admin", Email = "trainer@class.local", KeycloakUser = "trainer" },
                new Student { Name = "User", Email = "user@class.local", KeycloakUser = "user" });
            _db.Courses.AddRange(
                new Course { Title = "REST API Basics", Fee = 15000, SeatsAvailable = 3 },
                new Course { Title = ".NET Deep Dive", Fee = 28000, SeatsAvailable = 2 });
            await _db.SaveChangesAsync();
            return Ok("Seeded: 2 students, 2 courses");
        }

        [HttpPost("enrollments")]
        public async Task<IActionResult> CreateEnrollment
            ([FromBody] Enrollment enrollment)
        {

            // Check for existing enrollment with the same StudentId and CourseId
            var duplicateEnrollment = await _db.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == enrollment.StudentId && e.CourseId == enrollment.CourseId);
            if (duplicateEnrollment != null)
                return Conflict("This student is already enrolled in this course.");

            // Add the new enrollment
            _db.Enrollments.Add(enrollment);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(CreateEnrollment),
                new { id = enrollment.Id }, enrollment);
        }

        [HttpPatch("students")]
        public async Task<IActionResult> UpdateStudentDetails(int studentId, SaveStudent s)
        {
            var existing = _db.Students.FirstOrDefault(e => e.Id == studentId);
            if (existing == null)
                return NoContent();
            existing.Name = s.Name;
            existing.Email = s.Email;
            await _db.SaveChangesAsync();
            return Ok("Student updated succesfully");
        }

        [HttpGet("searching")]
        public async Task<IActionResult> Searching(string? search)
        {
            if (search != null && search.Length > 0)
                return Ok(_db.Students.AsNoTracking()
                    .Where(s => s.Name.ToLower().Contains(search) || s.Email.ToLower().Contains(search)));
            else
                return Ok(_db.Students.AsNoTracking());

            //page = Math.Clamp(page, 1, 1000000); pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 50);
            //IQueryable<Practicals.Models.Day2.Student> q = _db.Students.AsNoTracking();

            //var total = await q.CountAsync(); // filtered total, not current page count
            //q = (sort ?? "").ToLowerInvariant() switch
            //{
            //    "name" => q.OrderBy(x => x.FullName).ThenBy(x => x.Id),
            //    "name_desc" => q.OrderByDescending(x => x.FullName).ThenBy(x => x.Id),
            //    _ => q.OrderBy(x => x.Id)
            //};
            //var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
            //    .Select(x => new StudentDto(x.Id, x.FullName, x.Email, x.Phone, x.DateOfBirth,
            //        x.Year, x.CourseId, x.Course.Title)).ToListAsync();
            //return new(items, page, pageSize, total);
        }

        [HttpGet("sorting")]
        public async Task<IActionResult> Sorting(string sort = "asc")
        {
            IQueryable<Practicals.Models.Day2.Student> q = _db.Students.AsNoTracking();
            q = (sort ?? "").ToLowerInvariant() switch
            {
                "asc" => q.OrderBy(x => x.Name).ThenBy(x => x.Id),
                "desc" => q.OrderByDescending(x => x.Name).ThenBy(x => x.Id),
                _ => q.OrderBy(x => x.Id)
            };
            return Ok(q.ToList());
        }

        [HttpGet("paging")]
        public async Task<IActionResult> Paging(
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 5)
        {
            // 1. Guard against invalid values & cap max page size to prevent abuse
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 5;
            if (pageSize > 50) pageSize = 50; // Max limit

            // 2. Base query with AsNoTracking for fast read-only queries
            var query = _db.Students.AsNoTracking();

            // 3. Get total records count for pagination metadata
            var totalCount = await query.CountAsync();

            // 4. OrderBy is REQUIRED for consistent pagination in SQL
            var items = await query
                .OrderBy(s => s.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new StudentDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Email = s.Email
                })
                .ToListAsync();

            // 5. Build and return result
            var result = new PagedResult<StudentDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(result);
        }

        [HttpGet("students/{id:int}/enrollments")]
        public async Task<IActionResult> EnrollmentsUnsafe(int id)
            => Ok(await _db.Enrollments.AsNoTracking().Where(e => e.StudentId == id)
        .Select(e => new { e.Id, e.CourseId, e.AtUtc }).ToListAsync());


    }
}
