using Microsoft.AspNetCore.Mvc;

namespace PetClinics.Controllers
{
    [ApiController]
    [Route("/")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("PetClinics API is running.");
        }
    }
}
