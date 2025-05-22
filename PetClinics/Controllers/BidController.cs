using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinics.Models;
using static PetClinics.Controllers.BidController;

namespace PetClinics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    /// <summary>
    /// Контроллер для работы с заявками (Bids).
    /// </summary>
    public class BidController : Controller
    {
        private readonly UserManager<ExtendedUser> _userManager;
        private readonly ClinicDbContext _context;
        private readonly Guid _sterilizationId = new Guid("E7B915EE-041D-49BD-AD58-72690158C011");

        /// <summary>
        /// Конструктор контроллера заявок.
        /// </summary>
        /// <param name="userManager">Менеджер пользователей.</param>
        /// <param name="context">Контекст базы данных клиники.</param>
        public BidController(UserManager<ExtendedUser> userManager, ClinicDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Модель данных для создания заявки.
        /// </summary>
        /// <param name="UserId">Идентификатор пользователя.</param>
        /// <param name="PetId">Идентификатор питомца.</param>
        /// <param name="VeterinarianId">Идентификатор ветеринара.</param>
        /// <param name="FavorsId">Идентификатор услуги.</param>
        /// <param name="DateOfAdmission">Дата записи на приём.</param>
        public record CreateBid(string UserId, Guid? PetId, Guid? VeterinarianId, Guid? FavorsId, DateTime DateOfAdmission);
        [HttpPost("CreateBid")]

        /// <summary>
        /// Создаёт новую заявку.
        /// </summary>
        /// <param name="CreateBid">Модель заявки для создания.</param>
        /// <returns>Результат выполнения операции.</returns>
        public async Task<IActionResult> CreateBids([FromBody] CreateBid createBid)
        {
            var validationResult = await ValidateBids(createBid);
            if (validationResult != null)
            {
                return validationResult;
            }

            var bid = new Bids
            {
                UserId = createBid.UserId,
                PetId = createBid.PetId,
                VeterinarianId = createBid.VeterinarianId,
                FavorsId = createBid.FavorsId,
                DateOfAdmission = createBid.DateOfAdmission
            };

            _context.Bids.Add(bid);

            var appointment = new VeterinarianSchedule
            {
                VeterinarianId = createBid.VeterinarianId.Value,
                AppointmentDate = createBid.DateOfAdmission
            };

            _context.VeterinarianSchedule.Add(appointment);

            await _context.SaveChangesAsync();

            return Ok("Заявка успешно создана.");
        }

        /// <summary>
        /// Валидирует данные заявки перед её созданием.
        /// </summary>
        /// <param name="bidModel">Модель данных заявки.</param>
        /// <returns>Объект ошибки, если валидация не пройдена, или null, если всё корректно.</returns>
        private async Task<BadRequestObjectResult> ValidateBids(CreateBid bidModel)
        {
            
            var user = await _context.Users.FindAsync(bidModel.UserId);
            if (user is null)
            {
                return BadRequest("Пользователь не найден.");
            }

            var pet = await _context.Pets.FindAsync(bidModel.PetId);
            if (pet is null)
            {
                return BadRequest("Питомец не найден у данного пользователя.");
            }

            var veterinarian = await _context.Veterinarians.FindAsync(bidModel.VeterinarianId);
            if (veterinarian is null)
            {
                return BadRequest("Ветеринар не найден.");
            }

            var favor = await _context.Favors.FindAsync(bidModel.FavorsId);
            if (favor is null)
            {
                return BadRequest("Услуга не найдена.");
            }

            if (favor.Id == _sterilizationId)
            {
                var alreadyScheduled = await _context.Bids.AnyAsync(b =>
                    b.PetId == bidModel.PetId &&
                    b.FavorsId == bidModel.FavorsId &&
                    b.DateOfAdmission > DateTime.Now);

                if (alreadyScheduled)
                {
                    return BadRequest("Питомец уже записан на кастрацию.");
                }
            }

            var isDateTaken = await _context.VeterinarianSchedule
               .AnyAsync(ad => ad.VeterinarianId == bidModel.VeterinarianId && ad.AppointmentDate == bidModel.DateOfAdmission);

            if (isDateTaken)
            {
                return BadRequest("На указанное время уже есть запись к данному ветеринару.");
            }

            return null;
        }
        
    }
}
