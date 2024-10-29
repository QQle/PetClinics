namespace PetClinics.Models
{
    public class Pet
    {
        public Guid Id { get; set; }
        public string? Type { get; set; }
        public string? Gender { get; set; }
        public float? Age { get; set; }
        public bool Sterilized { get; set; }
        public bool Vaccinated { get; set; }
        public string? Name { get; set; }
        public ICollection<PetsOwners>? PetsOwners { get; set; } = new List<PetsOwners>();
        public ICollection<Bids>? Bids { get; set; } = new List<Bids>();
    }
}
