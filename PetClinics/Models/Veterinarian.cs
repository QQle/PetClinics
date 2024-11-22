namespace PetClinics.Models
{
    /// <summary>
    /// Класс, представляющий ветеринара.
    /// </summary>
    public class Veterinarian 
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Specialization { get; set; } 
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; } 
        public string? Address { get; set; } 
        public int YearsOfExperience { get; set; }
        public double Price { get; set; }
        public ICollection<Bids>? Bids { get; set; } = new List<Bids>();
    }
}
