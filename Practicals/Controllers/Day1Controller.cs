using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Practicals.Models;
using Practicals.Services.Day1;


namespace Practicals.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Day1Controller : ControllerBase
    {
        //[HttpGet]
        //public IActionResult GetProducts()
        //{
        //    var products = new[]
        //    {
        //        new { Id = 1, Name = "Laptop", Price = 75000 },
        //        new { Id = 2, Name = "Mouse", Price = 1500 }
        //    };

        //    return Ok(products);
        //}

        //[HttpGet("{id}")]
        //public IActionResult GetProduct(int id)
        //{
        //    var product = new
        //    {
        //        Id = id,
        //        Name = "Laptop",
        //        Price = 75000
        //    };

        //    return Ok(product);
        //}

        [HttpGet("~/api/demo/env")]
        public IActionResult GetEnvironment([FromServices] IConfiguration config,
                                    [FromServices] IWebHostEnvironment env)
        =>
            Ok(new
            {
                Environment = env.EnvironmentName,
                Greeting = config["Training:Greeting"]
            });

        private readonly ProductStore _store;
        public Day1Controller(ProductStore store) => _store = store;


        // GET /api/day1/products
        [HttpGet("products")]
        public ActionResult<List<Product>> GetProducts() => _store.Products;

        // GET /api/day1/products/2
        [HttpGet("products/{id:int}")]
        public ActionResult<Product> GetProduct(int id)
        {
            var p = _store.Products.FirstOrDefault(x => x.Id == id);
            return p is null ? NotFound() : p;
        }

        #region "Collections, Sub-Resources and Relationships"

        [HttpGet("products/{id:int}/reviews")]
        public ActionResult<List<Review>> GetReviews(int id)
        {
            if (_store.Products.All(p => p.Id != id))
                return NotFound();
            return _store.Reviews.Where(r => r.ProductId == id).ToList();
        }

        // POST /api/day1/products/1/reviews  -> create child under parent
        [HttpPost("products/{id:int}/reviews")]
        public ActionResult<Review> AddReview(int id, Review review)
        {
            if (_store.Products.All(p => p.Id != id))
                return NotFound();
            review.Id = _store.NextReviewId();
            review.ProductId = id;            // parent comes from the URL, never the body
            _store.Reviews.Add(review);
            return CreatedAtAction(nameof(GetReviews), new { id }, review);
        }


        #endregion

        #region "HTTP Methods, Safety and Idempotency"

        [HttpPost("products")]
        public ActionResult<Product> CreateProduct(Product product)
        {
            product.Id = _store.NextProductId();
            product.CreatedOn = DateTime.UtcNow;
            _store.Products.Add(product);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("products/{id:int}")]
        public IActionResult ReplaceProduct(int id, Product updated)
        {
            var existing = _store.Products.FirstOrDefault(p => p.Id == id);
            if (existing is null)
                return NotFound();
            existing.Name = updated.Name;
            existing.Price = updated.Price;
            existing.Category = updated.Category;
            existing.Stock = updated.Stock;   // every mutable field replaced
            return NoContent();
        }

        [HttpPatch("products/{id:int}")]
        public IActionResult PatchProduct(int id, UpdateProduct changes)
        {
            var existing = _store.Products.FirstOrDefault(p => p.Id == id);
            if (existing is null)
                return NotFound();
            if (changes.Name is not null) existing.Name = changes.Name;
            if (changes.Price is not null) existing.Price = changes.Price.Value;
            if (changes.Category is not null) existing.Category = changes.Category;
            if (changes.Stock is not null) existing.Stock = changes.Stock.Value;
            return NoContent();
        }

        [HttpDelete("products/{id:int}")]
        public IActionResult DeleteProduct(int id)
        {
            var p = _store.Products.FirstOrDefault(x => x.Id == id);
            if (p is null)
                return NotFound();     // already gone -> still a clean answer
            _store.Products.Remove(p);
            return NoContent();
        }

        #endregion

        #region "CRUD with Model Binding and DTO"

        private static readonly List<Student> _students =
        [
            new() { Id = 1, FullName = "Asha Mehta", Email = "asha@example.com", CourseId = 1, InternalNotes = "Scholarship" },
            new() { Id = 2, FullName = "Ravi Kumar", Email = "ravi@example.com", CourseId = 2, InternalNotes = "Late fees" }
        ];
        private static int _nextStudentId = 3;
        private static StudentDto ToConvertDto(Student s) =>
            new(s.Id, s.FullName, s.Email, s.CourseId);

        [HttpGet("~/api/students")]
        public ActionResult<IEnumerable<StudentDto>> GetStudents()
            => Ok(_students.Select(ToConvertDto));

        [HttpGet("~/api/students/{id:int}")]
        public ActionResult<StudentDto> GetStudentById(int id)
        {
            var s = _students.FirstOrDefault(x => x.Id == id);
            if (s is null) return NotFound();
            return Ok(ToConvertDto(s));
        }

        [HttpPost("~/api/students")]
        public ActionResult<StudentDto> CreateStudent([FromBody] CreateStudentDto dto)
        {
            if (_students.Any(x => x.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase)))
                return Conflict(new ProblemDetails
                {
                    Title = "Duplicate email",
                    Status = 409,
                    Detail = $"A student with email {dto.Email} already exists."
                });

            var s = new Student { Id = _nextStudentId++, FullName = dto.FullName, Email = dto.Email, CourseId = dto.CourseId };
            _students.Add(s);
            return CreatedAtAction(nameof(GetStudentById), new { id = s.Id }, ToConvertDto(s));   // 201 + Location
        }

        [HttpPut("~/api/students/{id:int}")]
        public IActionResult UpdateStudent(int id, [FromBody] UpdateStudentDto dto)
        {
            var s = _students.FirstOrDefault(x => x.Id == id);
            if (s is null) return NotFound();
            if (_students.Any(x => x.Id != id && x.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase)))
                return Conflict(new ProblemDetails { Title = "Duplicate email", Status = 409 });

            s.FullName = dto.FullName; s.Email = dto.Email; s.CourseId = dto.CourseId;
            return NoContent();
        }

        [HttpPatch("~/api/students/{id:int}")]
        public IActionResult PatchStudent(int id, [FromBody] PatchStudentDto dto)
        {
            var s = _students.FirstOrDefault(x => x.Id == id);
            if (s is null) return NotFound();
            if (dto.FullName is null && dto.Email is null)
                return BadRequest(new ProblemDetails { Title = "Nothing to update", Status = 400 });
            if (dto.Email is not null && _students.Any(x => x.Id != id && x.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase)))
                return Conflict(new ProblemDetails { Title = "Duplicate email", Status = 409 });

            if (dto.FullName is not null) s.FullName = dto.FullName;
            if (dto.Email is not null) s.Email = dto.Email;
            return NoContent();
        }

        [HttpDelete("~/api/students/{id:int}")]
        public IActionResult DeleteStudent(int id)
        {
            var s = _students.FirstOrDefault(x => x.Id == id);
            if (s is null) return NotFound();
            _students.Remove(s);
            return NoContent();
        }


        #endregion

        [HttpPost("~/api/addproductsdto")]
        public IActionResult AddProduct(CreateProductDto dto)
        {
            Product p = new Product()
            {
                Id = _store.NextProductId(),
                Name = dto.Name,
                Category = dto.Category,
                Price = dto.Price,
                Stock = dto.Stock,
                CreatedOn = DateTime.UtcNow
            };
            _store.Products.Add(p);
            return CreatedAtAction(nameof(AddProduct), new { id = p.Id });   // 201 + Location
        }

        [HttpPut("~/api/updateproductsdto")]
        public IActionResult UpdateProduct(int ProductId, UpdateProductDto dto)
        {
            var existing = _store.Products.FirstOrDefault(p => p.Id == ProductId);
            if (existing is null)
                return NotFound();
            existing.Name = dto.Name;
            existing.Price = dto.Price;
            existing.Category = dto.Category;
            existing.Stock = dto.stock;
            return Ok();
        }
    }
}
