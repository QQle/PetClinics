using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinics.Models;
using System.Security.Cryptography;

namespace PetClinics.Controllers
{

    [Route("api/[controller]")]
    [ApiController]

    /// <summary>
    /// Контроллер для работы с сущностью "Питомец" (Pet).
    /// </summary>
    public class PetController : Controller
    {
        private readonly ClinicDbContext _context;

        /// <summary>
        /// Конструктор контроллера питомцев.
        /// </summary>
        /// <param name="context">Контекст базы данных клиники.</param>
        public PetController(ClinicDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Возвращает список всех питомцев.
        /// </summary>
        /// <returns>Список всех питомцев в базе данных.</returns>
 
        [HttpGet("GetAllPets")]
        public async Task<IActionResult> GetAllPets()
        {
            var pets = await _context.Pets
                .ToListAsync();
            return Ok(pets);
        }

        /// <summary>
        /// Возвращает список питомцев, принадлежащих определённому владельцу.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя-владельца питомцев.</param>
        /// <returns>Список питомцев, принадлежащих указанному пользователю.</returns>

        [HttpPost("GetPetsByOwner")]
        public async Task<IActionResult> GetPetsByOwner([FromBody] string userId)
        {
            var pets = await _context.Pets
                .Where(p => p.PetsOwners.Any(up => up.UserId == userId)) 
                .ToListAsync();
            return Ok(pets);
        }

        /// <summary>
        /// Модель данных для добавления нового питомца.
        /// </summary>
        /// <param name="Type">Тип питомца (например, кошка, собака).</param>
        /// <param name="Gender">Пол питомца.</param>
        /// <param name="Age">Возраст питомца.</param>
        /// <param name="Sterilized">Флаг, указывающий, стерилизован ли питомец.</param>
        /// <param name="Vaccinated">Флаг, указывающий, привит ли питомец.</param>
        /// <param name="Name">Имя питомца.</param>
        /// <param name="OwnerId">Идентификатор владельца питомца.</param>
        public record InsertPet(string Type, string Gender, float Age, bool Sterilized, bool Vaccinated, string Name, string OwnerId);

        /// <summary>
        /// Добавляет нового питомца и связывает его с владельцем.
        /// </summary>
        /// <param name="pet">Модель данных для добавления питомца.</param>
        /// <returns>Результат выполнения операции.</returns>
        
        [HttpPost("InsertNewPet")]
        public async Task<IActionResult> InsertNewPet([FromBody] InsertPet pet)
        {
            var newPet = new Pet
            {
                Type = pet.Type,
                Gender = pet.Gender,
                Age = pet.Age,
                Sterilized = pet.Sterilized,
                Vaccinated = pet.Vaccinated,
                Name = pet.Name  
            };
            _context.Pets.Add(newPet);
            await _context.SaveChangesAsync();

            var petOwner = new PetsOwners
            {
                UserId = pet.OwnerId,
                PetId = newPet.Id
            };

            _context.PetsOwners.Add(petOwner);
            await _context.SaveChangesAsync();

            return Ok("Питомец успешно добавлен.");
        }


    }
}
