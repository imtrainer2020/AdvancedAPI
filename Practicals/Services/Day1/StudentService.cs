using Practicals.DTOs;
using Practicals.Models;

namespace Practicals.Services.Day1
{
    public interface IStudentService
    {
        List<StudentDto> GetAll(string? name);
        StudentDto? GetById(int id);
        StudentResult Create(CreateStudentDto dto);
        StudentResult Update(int id, UpdateStudentDto dto);
        StudentResult Rename(int id, RenameStudentDto dto);
        StudentResult Delete(int id);
    }
    public class StudentService : IStudentService
    {
        private static StudentDto Map(Student s)
            => new StudentDto(s.Id, s.Name, s.Email, s.CourseId);
        private static StudentResult Result(int status, string message = "")
            => new StudentResult { Status = status, Message = message };

        public List<StudentDto> GetAll(string? name)
        {
            var rows = Day1Store.Students.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(name))
                rows = rows.Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            return rows.Select(Map).ToList();
        }
        public StudentDto? GetById(int id)
        {
            var s = Day1Store.Students.FirstOrDefault(s => s.Id == id);
            return s == null ? null : Map(s);
        }
        private static StudentResult? Validate(string email, int courseId, int exceptId = 0)
        {
            if (!Day1Store.Courses.Any(c => c.Id == courseId))
                return Result(400, "Choose an existing course.");
            if (Day1Store.Students.Any(s => s.Id != exceptId &&
                s.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase)))
                return Result(409, "Email already exists.");
            return null;
        }
        public StudentResult Create(CreateStudentDto dto)
        {
            var error = Validate(dto.Email, dto.CourseId);
            if (error != null) return error;
            var s = new Student
            {
                Id = Day1Store.NextStudentId++,
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim(),
                CourseId = dto.CourseId
            };
            Day1Store.Students.Add(s);
            return new StudentResult { Status = 201, Student = Map(s) };
        }
        public StudentResult Update(int id, UpdateStudentDto dto)
        {
            var s = Day1Store.Students.FirstOrDefault(s => s.Id == id);
            if (s == null) return Result(404);
            var error = Validate(dto.Email, dto.CourseId, id);
            if (error != null) return error;
            s.Name = dto.Name.Trim(); s.Email = dto.Email.Trim(); s.CourseId = dto.CourseId;
            return Result(204);
        }
        public StudentResult Rename(int id, RenameStudentDto dto)
        {
            var s = Day1Store.Students.FirstOrDefault(s => s.Id == id);
            if (s == null) return Result(404);
            s.Name = dto.Name.Trim();
            return Result(204);
        }
        public StudentResult Delete(int id)
        {
            var s = Day1Store.Students.FirstOrDefault(s => s.Id == id);
            if (s == null) return Result(404);
            Day1Store.Students.Remove(s);
            return Result(204);
        }
    }

    public class StudentResult
    {
        public int Status { get; set; }
        public string Message { get; set; } = "";
        public StudentDto? Student { get; set; }
    }

}
