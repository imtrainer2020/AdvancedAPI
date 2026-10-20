namespace Practicals.Services.Day3
{
    public record StudentRead(int Id, string Name);
    public interface IStudentReader
    {
        Task<StudentRead?> Find(int id);
    }
    public class StudentLookup(IStudentReader reader)
    {
        public Task<StudentRead?> Get(int id) => reader.Find(id);
        public static bool CanEnroll(int age) => age >= 16;
    }

}
