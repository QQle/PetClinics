namespace PetClinics.Models
{
    /// <summary>
    /// Класс представляет ответ на авторизацию пользователя.
    /// </summary>
    public class AuthorizationResponse
    {
        public bool IsLoggedIn { get; set; } = false;
        public string JwtToken { get; set; } = string.Empty;
        public string RefreshToken { get; internal set; } = string.Empty;
    }
}
