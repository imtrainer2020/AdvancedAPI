using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practicals.DTOs;
using Practicals.Services.Day2;
using static Practicals.DTOs.Day2Dto;

namespace Practicals.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Day2Controller(StudentDbService students, CampusDb db) : ControllerBase
    {
        [HttpGet("students")]
        public async Task<ActionResult<List<StudentDto>>> Students() => Ok(await students.All());

        [HttpGet("students/{id:int}")]
        public async Task<ActionResult<StudentDto>> Student(int id)
        {
            var s = await students.One(id); return s is null ? NotFound() : Ok(s);
        }

        [HttpPost("students")]
        public async Task<ActionResult<StudentDto>> Create(StudentWrite dto)
        {
            var s = await students.Create(dto);
            return CreatedAtAction(nameof(Student), new { id = s.Id }, s);
        }

        [HttpPut("students/{id:int}")]
        public async Task<IActionResult> Update(int id, StudentUpdate dto) =>
            await students.Update(id, dto) ? NoContent() : NotFound();
        
        [HttpDelete("students/{id:int}")]
        public async Task<IActionResult> Delete(int id) =>
            await students.Delete(id) ? NoContent() : NotFound();

        [HttpGet("demos/tracking")]
        public async Task<IActionResult> Tracking()
        {
            db.ChangeTracker.Clear(); 
            _ = await db.Students.ToListAsync();
            var tracked = db.ChangeTracker.Entries().Count(); 
            db.ChangeTracker.Clear();
            _ = await db.Students.AsNoTracking().ToListAsync();
            return Ok(new { tracked, noTracking = db.ChangeTracker.Entries().Count() });
        }

        [HttpGet("demos/sql")]
        public IActionResult Sql() => Ok(db.Students.Where(x => x.Year == 1).OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.FullName }).ToQueryString());

        [HttpGet("courses/{id:int}/students")]
        public async Task<IActionResult> CourseStudents(int id)
        {
            var r = await db.Courses.AsNoTracking().Where(c => c.Id == id)
                .Select(c => new {
                    c.Id,
                    c.Title,
                    Students = c.Students.OrderBy(s => s.Id)
                    .Select(s => new { s.Id, s.FullName, s.Email }).ToList()
                }).FirstOrDefaultAsync();
            return r is null ? NotFound() : Ok(r);
        }

        [HttpGet("demos/include/{id:int}")]
        public async Task<IActionResult> IncludeDemo(int id)
        {
            var s = await db.Students.Include(x => x.Course)
                .Include(x => x.Enrollments).ThenInclude(e => e.Course).FirstOrDefaultAsync(x => x.Id == id);
            return s is null ? NotFound() : Ok(new
            {
                s.Id,
                s.FullName,
                Primary = s.Course.Title,
                Electives = s.Enrollments.Select(e => e.Course.Title).ToList()
            });
        }

        [HttpGet("demos/cycle/{id:int}")]
        public async Task<IActionResult> Cycle(int id) => Ok(await db.Courses
            .Include(c => c.Students).FirstOrDefaultAsync(c => c.Id == id));

        [HttpGet("demos/boom")]
        public IActionResult Boom() => throw new InvalidOperationException("Classroom crash.");
        [HttpPost("demos/duplicate")]
        public async Task<IActionResult> Duplicate()
        {
            // Deliberately bypass service precheck to test DB safety net.
            db.Students.Add(new Practicals.Models.Day2.Student
            {
                FullName = "Clone",
                Email = "asha@lab.local",
                Year = 1,
                CourseId = 1,
                DateOfBirth = new DateTime(2003, 1, 10)
            });
            await db.SaveChangesAsync(); return Ok();
        }

        [HttpGet("students_paging_sorting_searching")]
        public async Task<ActionResult<PagedResult<StudentsDto>>> Students(
            string? search, string? sort, int page = 1, int pageSize = 10)
        => Ok(await students.PagingSortingSearching(search, sort, page, pageSize));

    }
}
