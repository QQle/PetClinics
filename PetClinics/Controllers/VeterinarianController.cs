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
        private readonly ILogger<VeterinarianController> _logger;

        /// <summary>
        /// Конструктор контроллера ветеринаров.
        /// </summary>
        /// <param name="context">Контекст базы данных клиники.</param>
        /// <param name="userManager">Менеджер пользователей.</param>
        public VeterinarianController(ClinicDbContext context, UserManager<ExtendedUser> userManager, IEmail email, UserHelper userHelper, PetHelper petHelper, ILogger<VeterinarianController> logger)
        {
            _context = context;
            _userManager = userManager;
            _email = email;
            _userHelper = userHelper;
            _petHelper = petHelper;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            var bidInfo = await GetBidDetailsAsync(bidId);
            if (bidInfo == null)
                return NotFound("Заявка не найдена");

            var userName = await _userHelper.GetUserNameByUserId(bidInfo.UserId);
            var userEmail = await _userHelper.GetUserEmailById(bidInfo.UserId);
            var petName = await _petHelper.GetPetsByBidId(bidId);

            await _userHelper.SetIsAccepted(bidId);

            var fullPrice = bidInfo.FavorPrice + bidInfo.VeterinarianPrice;

            var model = BuildEmailModel(userName, petName, bidInfo, fullPrice);

            var htmlContent = await GenerateEmailBodyAsync(model);

            var emailDto = CreateEmailDto(userEmail, "Напоминание о приеме", htmlContent);

            _email.SendEmail(emailDto);
            return Ok("Вы приняли запись");
        }

        [HttpPost("NearestEntry")]
        public async Task<IActionResult> FindNearestEntry([FromBody] string specialization)
        {
            try
            {
                var veterinarians = await GetVeterinariansBySpecialization(specialization);
                var favorId = await GetFavorIdBySpecialization(specialization);

                var results = new List<dynamic>();

                foreach (var vet in veterinarians)
                {
                    var result = await BuildResultObject(vet, favorId);
                    results.Add(result);
                }

                var finalResult = results
                    .OrderBy(r => DateTime.ParseExact($"{r.NearestAvailableDate} {r.NearestAvailableTime}", "dd-MM-yyyy HH:mm", null))
                    .Take(1);

                return Ok(finalResult);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка при поиске ближайших записей.");
                return StatusCode(500, "Произошла ошибка при поиске записей.");
            }
        }

        private static Email CreateEmailDto(string sendTo, string subject, string body)
        {
            return new Email
            {
                SendTo = sendTo,
                Subject = subject,
                Body = body
            };
        }

        private async Task<BidInfoForEmailTemplate?> GetBidDetailsAsync(Guid bidId)
        {
            return await (from bid in _context.Bids
                          where bid.Id == bidId
                          join favor in _context.Favors on bid.FavorsId equals favor.Id
                          join vet in _context.Veterinarians on bid.VeterinarianId equals vet.Id
                          select new BidInfoForEmailTemplate
                          {
                              UserId = Guid.Parse(bid.UserId),
                              DateOfAdmission = bid.DateOfAdmission,
                              FavorName = favor.Name,
                              FavorPrice = (double)favor.BasePrice,
                              VeterinarianName = vet.Name,
                              VeterinarianPrice = vet.Price,
                              VeterinarianPhoto = vet.PhotoUrl
                          }).FirstOrDefaultAsync();
        }

        private static object BuildEmailModel(string userName, string petName, BidInfoForEmailTemplate bidInfo, double fullPrice)
        {
            return new
            {
                CustomerName = userName,
                PetName = petName,
                VeterenarianName = bidInfo.VeterinarianName,
                FavorName = bidInfo.FavorName,
                Price = fullPrice,
                DateOfAdmission = bidInfo.DateOfAdmission.ToString("yyyy-MM-dd"),
                TimeOfAdmission = bidInfo.DateOfAdmission.ToString("HH:mm"),
                VeterinarianPhoto = bidInfo.VeterinarianPhoto
            };
        }

        private async Task<string> GenerateEmailBodyAsync(object model)
        {
            var emailPagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "ConfirmationEmailSample.cshtml");
            var template = await System.IO.File.ReadAllTextAsync(emailPagePath);

            var razor = new RazorLightEngineBuilder()
                .UseMemoryCachingProvider()
                .Build();

            return await razor.CompileRenderStringAsync("template", template, model);
        }

        private async Task<List<Veterinarian>> GetVeterinariansBySpecialization(string specialization)
        {
            return await _context.Veterinarians
                .Where(v => v.Specialization.Trim().ToLower() == specialization.Trim().ToLower())
                .ToListAsync();
        }

        private async Task<Guid> GetFavorIdBySpecialization(string specialization)
        {
            return await _context.Favors
                .Where(f => f.Specialization == specialization)
                .Select(f => f.Id)
                .FirstOrDefaultAsync();
        }

        private async Task<DateTime> GetNearestSlotForVeterinarian(Guid veterinarianId)
        {
            var now = DateTime.UtcNow;

            var futureAppointments = await _context.VeterinarianSchedule
                .Where(s => s.VeterinarianId == veterinarianId && s.AppointmentDate > now)
                .OrderBy(s => s.AppointmentDate)
                .Select(s => s.AppointmentDate)
                .ToListAsync();

            if (futureAppointments.Any())
                return futureAppointments.First();

            var lastAppointment = await _context.VeterinarianSchedule
                .Where(s => s.VeterinarianId == veterinarianId)
                .OrderByDescending(s => s.AppointmentDate)
                .Select(s => s.AppointmentDate)
                .FirstOrDefaultAsync();

            if (lastAppointment == default)
                return now.Date.AddDays(1).AddHours(9);

            var oneHourAfter = lastAppointment.AddHours(1);
            return oneHourAfter < now ? now.Date.AddDays(1).AddHours(9) : oneHourAfter;
        }

        private async Task<dynamic> BuildResultObject(Veterinarian vet, Guid favorId)
        {
            var nearestSlot = await GetNearestSlotForVeterinarian(vet.Id);

            return new
            {
                VeterinarianId = vet.Id,
                VeterinarianName = vet.Name,
                Specialization = vet.Specialization,
                Favor = favorId,
                NearestAvailableDate = nearestSlot.ToString("dd-MM-yyyy"),
                NearestAvailableTime = nearestSlot.ToString("HH:mm")
            };
        }

    }
}
