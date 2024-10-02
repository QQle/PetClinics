namespace PetClinics.Models
{
    public class AuthorizationResponse
    {
        public bool IsLoggedIn { get; set; } = false;
        public string JwtToken { get; set; } = string.Empty;
        public string RefreshToken { get; internal set; } = string.Empty;
    }
}
