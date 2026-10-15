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



        [HttpGet("collection-example/units")]
        public IActionResult GetUnits()
        {
            return Ok(Day1.Units);
        }

        [HttpGet("collection-example/units/{unitId}")]
        public IActionResult GetUnit(int unitId)
        {
            var unit = Day1.Units.FirstOrDefault(u => u.Id == unitId);
            if (unit == null) return NotFound();
            return Ok(unit);
        }

        [HttpGet("sub-resource-example/units/{unitId}/personnel")]
        public IActionResult GetUnitPersonnel(int unitId)
        {
            if (!Day1.Units.Any(u => u.Id == unitId)) return NotFound();

            var people = Day1.PersonnelList.Where(p => p.UnitId == unitId);
            return Ok(people);
        }

        [HttpGet("sub-resource-example/units/{unitId}/personnel/{personnelId}")]
        public IActionResult GetUnitPersonnelById(int unitId, int personnelId)
        {
            var person = Day1.PersonnelList
                .FirstOrDefault(p => p.UnitId == unitId && p.Id == personnelId);

            if (person == null) return NotFound();
            return Ok(person);
        }

        [HttpGet("relation-example/units/{unitId}/equipment")]
        public IActionResult GetUnitEquipment(int unitId)
        {
            if (!Day1.Units.Any(u => u.Id == unitId)) return NotFound();

            return Ok(Day1.EquipmentList.Where(e => e.UnitId == unitId));
        }

        [HttpGet("relation-example/units/{unitId}/summary")]
        public IActionResult GetUnitSummary(int unitId)
        {
            var unit = Day1.Units.FirstOrDefault(u => u.Id == unitId);
            if (unit == null) return NotFound();

            return Ok(new
            {
                unit.Id,
                unit.Name,
                PersonnelCount = Day1.PersonnelList.Count(p => p.UnitId == unitId),
                EquipmentCount = Day1.EquipmentList.Count(e => e.UnitId == unitId),
                Links = new
                {
                    Personnel = $"/api/Day1/units/{unitId}/personnel",
                    Equipment = $"/api/Day1/units/{unitId}/equipment"
                }
            });
        }
    }
}
