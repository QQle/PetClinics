using Microsoft.AspNetCore.Identity;

namespace PetClinics.Models
{
    public class ExtendedUser : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
    }
}
