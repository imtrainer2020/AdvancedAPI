using System.ComponentModel.DataAnnotations;

namespace Practicals.Models
{
    public class Products
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string Category { get; set; } = "General";
        public int Stock { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

    public record CreateProductDto(string Name, decimal Price, string Category, int Stock);

    public record UpdateProductDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public decimal Price { get; init; }
        public int Stock { get; init; }


    }


}
