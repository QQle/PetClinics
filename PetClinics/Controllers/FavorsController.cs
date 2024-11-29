using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PetClinics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavorsController : Controller
    {
        private readonly ClinicDbContext _context;
        public FavorsController(ClinicDbContext clinicDbContext)
        {
            _context = clinicDbContext;
        }

        [HttpGet("GetAllFavors")]
        public async Task<IActionResult> GetAllFavors()
        {
            var favors = await _context.Favors
                .Select(b=>new
                {
                    Title = b.Name,
                    Description = b.Description,
                    BasePrice = b.BasePrice,
                    Specialization = b.Specialization
                })
                .ToListAsync();
            return Ok(favors);
        }
    }
}
