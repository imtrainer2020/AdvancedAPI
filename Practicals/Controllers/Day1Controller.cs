using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Practicals.Models;


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
            var product = new
            {
                Id = id,
                Name = "Laptop",
                Price = 75000
            };

            return Ok(product);
        }
    }
}
