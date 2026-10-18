using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Practicals.DTOs;
using Practicals.Models;
using Practicals.Services.Day1;
using static Practicals.Services.Day1.LifetimeDemo;


namespace Practicals.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Day1Controller : ControllerBase
    {
        [HttpGet]
        public IActionResult GetProducts()
        {
            var products = new[]
            {
                new { Id = 1, Name = "Laptop", Price = 75000 },
                new { Id = 2, Name = "Mouse", Price = 1500 }
            };

            return Ok(products);
        }

        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            if (id != 1 && id != 2) return NotFound();

            return Ok(new
            {
                Id = id,
                Name = id == 1 ? "Laptop" : "Mouse",
                Price = id == 1 ? 75000 : 1500
            });

        }

        [HttpGet("demo/environment")]
        public IActionResult EnvironmentDemo([FromServices] IConfiguration config,
            [FromServices] IWebHostEnvironment environment)
        {
            return Ok(new
            {
                environment = environment.EnvironmentName,
                message = config["Day1Message"]
            });
        }

        //[HttpGet("demo/error")]
        //public IActionResult ErrorDemo()
        //{
        //    throw new InvalidOperationException("Intentional Day 1 classroom error.");
        //}

        [HttpGet("units")]
        public IActionResult GetUnits() => Ok(Day1Store.Units);

        [HttpGet("units/{id:int}")]
        public IActionResult GetUnit(int id)
        {
            var unit = Day1Store.Units.FirstOrDefault(u => u.Id == id);
            if (unit == null) return NotFound();
            return Ok(unit);
        }

        [HttpGet("units/{unitId:int}/personnel")]
        public IActionResult GetUnitPersonnel(int unitId)
        {
            if (!Day1Store.Units.Any(u => u.Id == unitId)) return NotFound();
            return Ok(Day1Store.People.Where(p => p.UnitId == unitId).ToList());
        }

        [HttpGet("units/{unitId:int}/summary")]
        public IActionResult GetUnitSummary(int unitId)
        {
            var unit = Day1Store.Units.FirstOrDefault(u => u.Id == unitId);
            if (unit == null) return NotFound();
            return Ok(new
            {
                unit.Id,
                unit.Name,
                personnelCount = Day1Store.People.Count(p => p.UnitId == unitId)
            });
        }

        // These two responses simulate statuses only. NO authentication is installed here.
        [HttpGet("demo/status/{code:int}")]
        public IActionResult DemoStatus(int code)
        {
            if (code == 401)
            {
                Response.Headers["WWW-Authenticate"] = "Bearer";
                return Unauthorized(new { message = "Simulated: no valid identity" });
            }
            if (code == 403) return StatusCode(403, new { message = "Simulated: not permitted" });
            return BadRequest(new { message = "Choose 401 or 403 for this demo." });
        }

        private static StudentDto ToDto(Student s)
    => new StudentDto(s.Id, s.Name, s.Email, s.CourseId);

        [HttpGet("demo/binding/{id:int}")]
        public IActionResult BindingDemo([FromRoute] int id,
            [FromQuery] string? name, [FromHeader(Name = "X-Client")] string? client)
        {
            return Ok(new { id, name, client });
        }

        [HttpGet("students")]
        public ActionResult<List<StudentDto>> GetStudents([FromQuery] string? name)
        {
            var students = Day1Store.Students.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(name))
                students = students.Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            return Ok(students.Select(ToDto).ToList());
        }

        [HttpGet("students/{id:int}")]
        public ActionResult<StudentDto> GetStudent(int id)
        {
            var student = Day1Store.Students.FirstOrDefault(s => s.Id == id);
            if (student == null) return NotFound();
            return Ok(ToDto(student));
        }

        [HttpPost("students")]
        public ActionResult<StudentDto> CreateStudent([FromBody] CreateStudentDto dto)
        {
            if (!Day1Store.Courses.Any(c => c.Id == dto.CourseId))
                return BadRequest(new { message = "Choose an existing course." });
            if (Day1Store.Students.Any(s => s.Email.Equals(dto.Email.Trim(), StringComparison.OrdinalIgnoreCase)))
                return Conflict(new { message = "Email already exists." });

            var student = new Student
            {
                Id = Day1Store.NextStudentId++,
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim(),
                CourseId = dto.CourseId
            };
            Day1Store.Students.Add(student);
            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, ToDto(student));
        }

        [HttpPut("students/{id:int}")]
        public IActionResult UpdateStudent(int id, [FromBody] UpdateStudentDto dto)
        {
            var student = Day1Store.Students.FirstOrDefault(s => s.Id == id);
            if (student == null) return NotFound();
            if (!Day1Store.Courses.Any(c => c.Id == dto.CourseId))
                return BadRequest(new { message = "Choose an existing course." });
            if (Day1Store.Students.Any(s => s.Id != id &&
                s.Email.Equals(dto.Email.Trim(), StringComparison.OrdinalIgnoreCase)))
                return Conflict(new { message = "Email already exists." });

            student.Name = dto.Name.Trim();
            student.Email = dto.Email.Trim();
            student.CourseId = dto.CourseId;
            return NoContent();
        }

        [HttpPatch("students/{id:int}/name")]
        public IActionResult RenameStudent(int id, [FromBody] RenameStudentDto dto)
        {
            var student = Day1Store.Students.FirstOrDefault(s => s.Id == id);
            if (student == null) return NotFound();
            student.Name = dto.Name.Trim();
            return NoContent();
        }

        [HttpDelete("students/{id:int}")]
        public IActionResult DeleteStudent(int id)
        {
            var student = Day1Store.Students.FirstOrDefault(s => s.Id == id);
            if (student == null) return NotFound();
            Day1Store.Students.Remove(student);
            return NoContent();
        }

        [HttpGet("demo/lifetimes")]
        public IActionResult Lifetimes(
            [FromServices] TransientStamp t1, [FromServices] TransientStamp t2,
            [FromServices] ScopedStamp s1, [FromServices] ScopedStamp s2,
            [FromServices] SingletonStamp a1, [FromServices] SingletonStamp a2)
        {
            return Ok(new
            {
                transient = new[] { t1.Id, t2.Id },
                scoped = new[] { s1.Id, s2.Id },
                singleton = new[] { a1.Id, a2.Id }
            });
        }

        private readonly IStudentService _students;
        public Day1Controller(IStudentService students)
        {
            _students = students;
        }

        [HttpGet("students_di")]
        public ActionResult<List<StudentDto>> GetStudentsDI([FromQuery] string? name)
            => Ok(_students.GetAll(name));

        [HttpGet("students_di/{id:int}")]
        public ActionResult<StudentDto> GetStudentDI(int id)
        {
            var student = _students.GetById(id);
            return student == null ? NotFound() : Ok(student);
        }

        private IActionResult ToHttp(StudentResult result)
        {
            if (result.Status == 204) return NoContent();
            if (result.Status == 404) return NotFound();
            return StatusCode(result.Status, new { message = result.Message });
        }

        [HttpPost("students_di")]
        public ActionResult<StudentDto> CreateStudentDI([FromBody] CreateStudentDto dto)
        {
            var result = _students.Create(dto);
            if (result.Status == 201)
                return CreatedAtAction(nameof(GetStudent),
                    new { id = result.Student!.Id }, result.Student);
            return StatusCode(result.Status, new { message = result.Message });
        }

        [HttpPut("students_di/{id:int}")]
        public IActionResult UpdateStudentDI(int id, [FromBody] UpdateStudentDto dto)
            => ToHttp(_students.Update(id, dto));

        [HttpPatch("students_di/{id:int}/name")]
        public IActionResult RenameStudentDI(int id, [FromBody] RenameStudentDto dto)
            => ToHttp(_students.Rename(id, dto));

        [HttpDelete("students_di/{id:int}")]
        public IActionResult DeleteStudentDI(int id)
            => ToHttp(_students.Delete(id));


    }
}
