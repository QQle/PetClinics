namespace PetClinics.Models
{
    /// <summary>
    /// Класс, представляющий запрос на обновление токенов (JWT и Refresh Token).
    /// </summary>
    public class RefreshToken
    {
        public string? JwtToken { get; set; }
        public string? RefreshTokens { get; set; }
    }
}
