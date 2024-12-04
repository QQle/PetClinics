using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinics.Models;
using PetClinics.Services;
using RazorLight;
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
        private readonly IEmail _email;
        private readonly UserHelper _userHelper;
        private readonly PetHelper _petHelper;

        /// <summary>
        /// Конструктор контроллера ветеринаров.
        /// </summary>
        /// <param name="context">Контекст базы данных клиники.</param>
        /// <param name="userManager">Менеджер пользователей.</param>
        public VeterinarianController(ClinicDbContext context, UserManager<ExtendedUser> userManager, IEmail email, UserHelper userHelper, PetHelper petHelper)
        {
            _context = context;
            _userManager = userManager;
            _email = email;
            _userHelper = userHelper;
            _petHelper = petHelper;
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

        [HttpPost("GetVeterinarianBySpecialization")]
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
            .Include(b => b.User)       
            .Include(b => b.Pet)       
            .Include(b => b.Favors)     
            .OrderBy(b => b.DateOfAdmission) 
            .Select(b => new
            {
                BidId = b.Id,
                ClientName = b.User.UserName,     
                PetName = b.Pet.Name,           
                FavorName = b.Favors.Name,       
                DateOfAdmission = b.DateOfAdmission,
                Vaccinated = b.Pet.Vaccinated,
                Sterialized = b.Pet.Sterilized,
                Age = b.Pet.Age,
                PetType = b.Pet.Type,
                Gender = b.Pet.Gender,
                isAccept = b.IsAccepted

            })
            .ToListAsync();
            return Ok(bids);
        }

        public record ExtendetVeterinarian(Guid VeterinarianId, string Specialization, string Address, double YearsOfExperience, double Price);
        [HttpPatch("UpdateVeterinarianInfo")]
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

        [HttpPatch("AcceptBids")]
        public async Task<IActionResult> AcceptBids([FromBody] Guid bidId)
        {
            var currentUser = await _context.Bids
                .Where(b => b.Id == bidId)
                .Select(u=> Guid.Parse(u.UserId))
                .FirstAsync();
            var userName = await _userHelper.GetUserNameByUserId(currentUser);
            var pet = await _petHelper.GetPetsByBidId(bidId);
            var isSet = await _userHelper.SetIsAccepted(bidId);

            var model = new
            {
                CustomerName = userName,
                PetName = pet
                
            };

            var emailPagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "EmailSample.cshtml");

            var razor = new RazorLightEngineBuilder()
                .UseMemoryCachingProvider()
                .Build();

            var template = await System.IO.File.ReadAllTextAsync(emailPagePath);

            var htmlContent = await razor.CompileRenderStringAsync("template", template, model);

            string userEmail = await _userHelper.GetUserEmailById(currentUser);

            var emailDto = new Email
            {
                SendTo = userEmail,
                Subject = "Напоминание о приеме",
                Body = htmlContent
            };

            _email.SendEmail(emailDto);
            return Ok("Вы приняли записась");
        }

   

    }
}
