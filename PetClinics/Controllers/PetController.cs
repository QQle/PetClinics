using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinics.Models;

namespace PetClinics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetController : Controller
    {
        private readonly ClinicDbContext _context;

        public PetController(ClinicDbContext context)
        {
            _context = context;
        }
        [HttpGet("GetAllPets")]
        public async Task<IActionResult> GetAllPets()
        {
            var pets = await _context.Pets
                .ToListAsync();
            return Ok(pets);
        }

      [HttpGet("GetPetsByOwner")]
        public async Task<IActionResult> GetPetsByOwner([FromQuery] string userId)
        {
            var pets = await _context.Pets
                .Where(p => p.PetsOwners.Any(up => up.UserId == userId)) 
                .ToListAsync();
            return Ok(pets);
        }


    }
}
