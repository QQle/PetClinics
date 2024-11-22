using Microsoft.AspNetCore.Identity;

namespace PetClinics.Models
{
    /// <summary>
    /// Класс представляет пользователя системы с дополнительными данными.
    /// </summary>
    public class ExtendedUser : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
        public ICollection<PetsOwners>? PetsOwners { get; set; } = new List<PetsOwners>();
        public ICollection<Bids>? Bids { get; set; } = new List<Bids>();
    }
}
