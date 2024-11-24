using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinics.Models;

namespace PetClinics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    /// <summary>
    /// Контроллер для работы с сущностью "Ветеринар" (Veterinarian).
    /// </summary>
    public class VeterinarianController : Controller
    {
        private readonly UserManager<ExtendedUser> _userManager;
        private readonly ClinicDbContext _context;

        /// <summary>
        /// Конструктор контроллера ветеринаров.
        /// </summary>
        /// <param name="context">Контекст базы данных клиники.</param>
        /// <param name="userManager">Менеджер пользователей.</param>
        public VeterinarianController(ClinicDbContext context, UserManager<ExtendedUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Возвращает список всех ветеринаров.
        /// </summary>
        /// <returns>Список всех ветеринаров в базе данных.</returns>

        [HttpGet("GetAllVeterinarian")]
        public async Task<IActionResult> GetAllVeterinarian()
        {
            var veterinarians = await _context.Veterinarians
                .ToListAsync();
            return Ok(veterinarians);
        }

        /// <summary>
        /// Возвращает список ветеринаров по указанной специализации.
        /// </summary>
        /// <param name="specialization">Специализация ветеринаров (например, хирург, терапевт).</param>
        /// <returns>Список ветеринаров с указанной специализацией.</returns>

        [HttpGet("GetVeterinarianBySpecialization")]
        public async Task<IActionResult> GetVeterinarianBySpecialization([FromBody] string specialization)
        {
            var pets = await _context.Veterinarians
              .Where(v => v.Specialization == specialization)
              .ToListAsync();
            return Ok(pets);
        }

        /// <summary>
        /// Возвращает список заявок, связанных с указанным ветеринаром.
        /// </summary>
        /// <param name="veterinarianId">Идентификатор ветеринара.</param>
        /// <returns>Список заявок, связанных с ветеринаром, отсортированный по названию услуг.</returns>

        [HttpPost("GetVeterinarianBids")]
        public async Task<IActionResult> GetVeterinarianBids([FromBody] Guid veterinarianId)
        {
            var bids = await _context.Bids
            .Where(b => b.VeterinarianId == veterinarianId)
            .OrderBy(b => b.Favors.Name)
            .ToListAsync();
            return Ok(bids);
        }

        public record ExtendetVeterinarian(Guid VeterinarianId, string Specialization, string Address, double YearsOfExperience, double Price);
        [HttpPost("UpdateVeterinarianInfo")]
        public async Task<IActionResult> UpdateVeterinarianInfo([FromBody] ExtendetVeterinarian veterinarian)
        {
            var existingVeterinarian = await _context.Veterinarians
        .FirstOrDefaultAsync(v => v.Id == veterinarian.VeterinarianId);

            if (existingVeterinarian == null)
            {
                return NotFound(new { Message = "Ветеринар с указанным идентификатором не найден." });
            }

            existingVeterinarian.Specialization = veterinarian.Specialization;
            existingVeterinarian.Address = veterinarian.Address;
            existingVeterinarian.YearsOfExperience = (int)veterinarian.YearsOfExperience;
            existingVeterinarian.Price = veterinarian.Price;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Информация о ветеринаре успешно обновлена." });
        }

    }
}
