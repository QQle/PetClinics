using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinics.Models;
using PetClinics.Services;

namespace PetClinics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    /// <summary>
    /// Контроллер для управления операциями пользователей.
    /// </summary>
    public class UserController : ControllerBase
    {
        private readonly IAuthentication _authService;
        private readonly UserManager<ExtendedUser> _userManager;
        private readonly ClinicDbContext _context;

        /// <summary>
        /// Конструктор контроллера пользователей.
        /// </summary>
        /// <param name="authService">Сервис аутентификации и авторизации.</param>
        /// <param name="context">Контекст базы данных клиники.</param>
        /// <param name="userManager">Менеджер пользователей.</param>
        public UserController(IAuthentication authService, ClinicDbContext context, UserManager<ExtendedUser> userManager)
        {
            _authService = authService;
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Регистрирует нового пользователя.
        /// </summary>
        /// <param name="user">Модель данных для регистрации пользователя.</param>
        /// <returns>Результат выполнения операции.</returns>

        [HttpPost("Registration")]
        public async Task<IActionResult> RegistrationUser([FromBody] RegistrationUser user)
        {
            if (await _authService.Registration(user))
            {
                return Ok("Вы успешно зарегистрировались");
            }
            return BadRequest("что-то пошло не так");
        }

        /// <summary>
        /// Выполняет вход пользователя в систему.
        /// </summary>
        /// <param name="user">Модель данных для авторизации пользователя.</param>
        /// <returns>JWT токен, Refresh токен и идентификатор текущего пользователя.</returns>
  
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginUser user)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var loginResult = await _authService.Login(user);
            var currentUser = await _context.Users
                .Where(x => x.UserName == user.UserName)
                .Select(x => x.Id)
                .FirstAsync();
               
            var resultObject = new
            {
                Jwt = loginResult.JwtToken,
                Refresh = loginResult.RefreshToken,
                CurrentUser = currentUser
            };
            if (loginResult.IsLoggedIn)
            {
                return Ok(resultObject);
            }
            return Unauthorized();
        }

        /// <summary>
        /// Выполняет выход пользователя из системы.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя, выполняющего выход.</param>
        /// <returns>Результат выполнения операции.</returns>

        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody]string userId)
        {

            var currentUser = await _userManager.FindByIdAsync(userId);
            if (currentUser == null)
            {
                return BadRequest("Пользователь не найден");
            }
            currentUser.RefreshToken = null;
            currentUser.RefreshTokenExpiry = DateTime.MinValue;
            var updateResult = await _userManager.UpdateAsync(currentUser);
            if (!updateResult.Succeeded)
            {
                return StatusCode(500, "Не удалось обновить пользователя");
            }
            return Ok("Вы успешно вышли из системы");
        }

        /// <summary>
        /// Обновляет токен доступа с использованием токена обновления.
        /// </summary>
        /// <param name="model">Модель данных для обновления токена.</param>
        /// <returns>Обновлённый JWT токен или ошибка авторизации.</returns>

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshToken model)
        {
            var loginResult = await _authService.RefreshToken(model);
            if (loginResult.IsLoggedIn)
            {
                return Ok(loginResult);
            }
            return Unauthorized();
        }
    }
}
