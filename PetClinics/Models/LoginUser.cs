namespace PetClinics.Models
{
    /// <summary>
    /// Класс представляет данные пользователя для входа в систему.
    /// </summary>
    public class LoginUser
    {
        public string? UserName { get; set; }
        public string Password { get; set; } = string.Empty;

    }
}
