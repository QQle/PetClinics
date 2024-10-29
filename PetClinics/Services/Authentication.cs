using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PetClinics.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PetClinics.Services
{
    public class Authentication : IAuthentication
    {
        private readonly UserManager<ExtendedUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly ILogger<Authentication> _logger;
        public Authentication(UserManager<ExtendedUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config, ILogger<Authentication> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
            _logger = logger;
        }

        public async Task<bool> Registration(RegistrationUser user)
        {

            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Пользователь с данной почтой уже существует ");
                return false;
            }

            var identityUser = new ExtendedUser
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(identityUser, user.Password);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(user.Role) && await _roleManager.RoleExistsAsync(user.Role))
                {
                    await _userManager.AddToRoleAsync(identityUser, user.Role);
                }
                else
                {
                    _logger.LogWarning("Данная роль не существует");
                }

                return true;
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError("Ошибка процесса регистрации: {Error}", error.Description);
            }

            return false;

        }
        public async Task<AuthorizationResponse> Login(LoginUser user)
        {
            var response = new AuthorizationResponse();
            var identityUser = await _userManager.FindByNameAsync(user.UserName);

            if (identityUser is null || (await _userManager.CheckPasswordAsync(identityUser, user.Password)) == false)
            {
                return response;
            }

            response.IsLoggedIn = true;
            response.JwtToken = this.GenerateTokenString(identityUser.Email);
            response.RefreshToken = this.GenerateRefreshTokenString();
            identityUser.RefreshToken = response.RefreshToken;
            identityUser.RefreshTokenExpiry = DateTime.Now.AddHours(12);
            await _userManager.UpdateAsync(identityUser);

            return response;
        }
        public async Task<AuthorizationResponse> RefreshToken(RefreshToken model)
        {
            var principal = GetTokenPrincipal(model.JwtToken);

            var response = new AuthorizationResponse();
            if (principal?.Identity?.Name is null)
            {
                _logger.LogWarning("Principal or Identity or Name is null");
                return response;
            }

            var identityUser = await _userManager.FindByEmailAsync(principal.Identity.Name);

            if (identityUser is null)
            {
                _logger.LogWarning($"Пользователь не найден: {principal.Identity.Name}");
                return response;
            }

            if (identityUser.RefreshToken != model.RefreshTokens || identityUser.RefreshTokenExpiry < DateTime.Now)
            {
                _logger.LogWarning($"Refresh token mismatch or expired for user: {principal.Identity.Name}");
                return response;
            }

            response.IsLoggedIn = true;
            response.JwtToken = this.GenerateTokenString(identityUser.UserName);
            response.RefreshToken = this.GenerateRefreshTokenString();

            identityUser.RefreshToken = response.RefreshToken;
            identityUser.RefreshTokenExpiry = DateTime.Now.AddDays(1);
            await _userManager.UpdateAsync(identityUser);

            return response;
        }
        private ClaimsPrincipal? GetTokenPrincipal(string token)
        {

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:Key").Value));

            var validation = new TokenValidationParameters
            {
                IssuerSigningKey = securityKey,
                ValidateLifetime = false,
                ValidateActor = false,
                ValidateIssuer = false,
                ValidateAudience = false,
            };
            return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
        }

        private string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[64];

            using (var numberGenerator = RandomNumberGenerator.Create())
            {
                numberGenerator.GetBytes(randomNumber);
            }

            return Convert.ToBase64String(randomNumber);
        }

        private string GenerateTokenString(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,userName),
                new Claim(ClaimTypes.Role,"Admin"),
            };

            var staticKey = _config.GetSection("Jwt:Key").Value;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(staticKey));
            var signingCred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var securityToken = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(1),
                signingCredentials: signingCred
                );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(securityToken);
            return tokenString;
        }
    }
}
