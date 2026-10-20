using Microsoft.EntityFrameworkCore;
using Practicals.DTOs;
using static Practicals.DTOs.Day2Dto;
using static Practicals.Models.Day2;

namespace Practicals.Services.Day2
{
    public class StudentDbService(CampusDb db, ILogger<StudentDbService> log)
    {
        public Task<List<StudentsDto>> All() => db.Students.AsNoTracking().OrderBy(x => x.Id)
        .Select(x => new StudentsDto(x.Id, x.FullName, x.Email, x.Phone, x.DateOfBirth,
            x.Year, x.CourseId, x.Course.Title)).ToListAsync();

        public Task<StudentsDto?> One(int id) => db.Students.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new StudentsDto(x.Id, x.FullName, x.Email, x.Phone, x.DateOfBirth,
                x.Year, x.CourseId, x.Course.Title)).FirstOrDefaultAsync();

        private async Task Check(StudentWrite d, int exceptId = 0)
        {
            if (d.DateOfBirth is null || d.DateOfBirth.Value.Date < new DateTime(1900, 1, 1))
                throw new RuleException(422, "Enter birth date from1900 onwards.");
            if (d.DateOfBirth.Value.Date > DateTime.Today.AddYears(-16))
                throw new RuleException(422, "Minimum age is 16.");
            if (!await db.Courses.AnyAsync(x => x.Id == d.CourseId))
                throw new RuleException(422, "Choose an existing course.");

            var e = d.Email.Trim().ToLowerInvariant();
            if (await db.Students.AnyAsync(x => x.Email == e && x.Id != exceptId))
                throw new RuleException(409, "Email already exists.");
        }
        private static void Copy(Student s, StudentWrite d)
        {
            s.FullName = d.FullName.Trim();
            s.Email = d.Email.Trim().ToLowerInvariant();
            s.DateOfBirth = d.DateOfBirth!.Value;
            s.Phone = d.Phone;
            s.Year = d.Year;
            s.CourseId = d.CourseId;
        }
        public async Task<StudentsDto> Create(StudentWrite d)
        {
            await Check(d);
            var s = new Student();
            Copy(s, d);
            db.Students.Add(s);
            await db.SaveChangesAsync();
            log.LogInformation("Created student {StudentId}", s.Id);
            return (await One(s.Id))!;
        }
        public async Task<bool> Update(int id, StudentUpdate d)
        {
            var s = await db.Students.FindAsync(id);
            if (s is null)
                return false;
            await Check(d, id);
            Copy(s, d);
            await db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> Delete(int id)
        {
            var s = await db.Students.FindAsync(id);
            if (s is null)
                return false;
            db.Students.Remove(s);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResult<StudentsDto>> PagingSortingSearching(string? search, string? sort, int page, int pageSize)
        {
            page = Math.Clamp(page, 1, 1000000); 
            pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 50);
            IQueryable<Student> q = db.Students.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var text = search.Trim(); q = q.Where(x => x.FullName.Contains(text) || x.Email.Contains(text));
            }
            var total = await q.CountAsync(); // filtered total, not current page count
            q = (sort ?? "").ToLowerInvariant() switch
            {
                "name" => q.OrderBy(x => x.FullName).ThenBy(x => x.Id),
                "name_desc" => q.OrderByDescending(x => x.FullName).ThenBy(x => x.Id),
                _ => q.OrderBy(x => x.Id)
            };
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new StudentsDto(x.Id, x.FullName, x.Email, x.Phone, x.DateOfBirth,
                    x.Year, x.CourseId, x.Course.Title)).ToListAsync();
            return new(items, page, pageSize, total);
        }

    }
    public class RuleException(int status, string message) : Exception(message)
    {
        public int Status { get; } = status;
    }

}
