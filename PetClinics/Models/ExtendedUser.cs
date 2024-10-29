using Microsoft.AspNetCore.Identity;

namespace PetClinics.Models
{
    public class ExtendedUser : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
        public ICollection<PetsOwners>? PetsOwners { get; set; } = new List<PetsOwners>();
        public ICollection<Bids>? Bids { get; set; } = new List<Bids>();
    }
}
