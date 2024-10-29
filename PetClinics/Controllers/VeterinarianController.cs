using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinics.Models;

namespace PetClinics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VeterinarianController : Controller
    {
        private readonly UserManager<ExtendedUser> _userManager;
        private readonly ClinicDbContext _context;
        public VeterinarianController(ClinicDbContext context, UserManager<ExtendedUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("GetAllVeterinarian")]
        public async Task<IActionResult> GetAllVeterinarian()
        {
            var pets = await _context.Veterinarians
                .ToListAsync();
            return Ok(pets);
        }

        [HttpGet("GetVeterinarianBySpecialization")]
        public async Task<IActionResult> GetVeterinarianBySpecialization([FromBody] string specialization)
        {
            var pets = await _context.Veterinarians
              .Where(v => v.Specialization == specialization) 
              .ToListAsync();
            return Ok(pets);
        }
    }
}
