using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinics.Models;
using PetClinics.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

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
        private readonly UserHelper _userHelper;
        private readonly UserManager<ExtendedUser> _userManager;
        private readonly ClinicDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Конструктор контроллера пользователей.
        /// </summary>
        /// <param name="authService">Сервис аутентификации и авторизации.</param>
        /// <param name="context">Контекст базы данных клиники.</param>
        /// <param name="userManager">Менеджер пользователей.</param>
        public UserController(IAuthentication authService, ClinicDbContext context, UserManager<ExtendedUser> userManager, RoleManager<IdentityRole> roleManager, UserHelper helper)
        {
            _authService = authService;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _userHelper = helper;
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
            Guid currentUser = await _userHelper.GetUserIdByUserName(user.UserName);
            var userRole = await _userHelper.GetUserRole(currentUser);
            var hasVeterinarianRole = await _userHelper.HasVeterinarionRole(currentUser);
            object resultObject;
            
            if (hasVeterinarianRole)
            {
                var extendedInfoFull = await _userHelper.CheckForExtendedUserInfo(currentUser, hasVeterinarianRole);

                resultObject = new
                {
                    Jwt = loginResult.JwtToken,
                    Refresh = loginResult.RefreshToken,
                    CurrentUser = currentUser,
                    Role = userRole,
                    ExtendedInfo = extendedInfoFull
                };
            }
            else
            {
                resultObject = new
                {
                    Jwt = loginResult.JwtToken,
                    Refresh = loginResult.RefreshToken,
                    CurrentUser = currentUser,
                    Role = userRole
                };
            }
            if (loginResult.IsLoggedIn)
            {
                return Ok(resultObject);
            }
            return Unauthorized();
        }

        [HttpPost("GetAllUserBids")]
        public async Task<IActionResult> GetAllUserBids([FromBody] Guid userId)
        {
            var currentUser = await _userHelper.ValidateUser(userId);
            if(string.IsNullOrEmpty(currentUser))
            {
                return BadRequest();
            }
            var userBids = await _userHelper.GetUserBids(userId);
            return Ok(userBids);
        }


        /// <summary>
        /// Выполняет выход пользователя из системы.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя, выполняющего выход.</param>
        /// <returns>Результат выполнения операции.</returns>

        [HttpPost("Logout")]
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
