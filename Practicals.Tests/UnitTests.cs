using Practicals.Services.Day3;
using System.ComponentModel.DataAnnotations;
using static Practicals.Models.Day3;

namespace Practicals.Tests
{
    public class UnitTests
    {

        private class FakeReader : IStudentReader
        {
            public Task<StudentRead?> Find(int id) =>
                Task.FromResult<StudentRead?>(id == 1 ? new StudentRead(1, "Asha") : null);
        }

        [Fact]
        public async Task ExistingStudent_IsReturned()
        {
            var service = new StudentLookup(new FakeReader());      // Arrange
            var result = await service.Get(1);                      // Act
            Assert.Equal("Asha", result!.Name);                     // Assert
        }

        [Theory]
        [InlineData(15, false)]
        [InlineData(16, true)]
        [InlineData(40, true)]
        public void CanEnroll_Boundary(int age, bool expected) =>
            Assert.Equal(expected, StudentLookup.CanEnroll(age));
        
        [Theory]
        [InlineData("", "pw", false)]
        [InlineData("student", "", false)]
        [InlineData("student", "pw", true)]
        public void LoginDto_Rules(string user, string pwd, bool expected)
        {
            var dto = new LoginDto { UserName = user, Password = pwd };
            var ok = Validator.TryValidateObject(dto, new ValidationContext(dto), new List<ValidationResult>(), true);
            Assert.Equal(expected, ok);
        }

    }
}
