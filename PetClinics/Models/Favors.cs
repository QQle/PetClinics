namespace PetClinics.Models
{
    /// <summary>
    /// Класс представляет услугу, предоставляемую в клинике.
    /// </summary>
    public class Favors
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }    
        public string? Description { get; set; }
        public double? BasePrice { get; set; }
        public string? Specialization { get; set; }
        public ICollection<Bids>? Bids { get; set; } = new List<Bids>();
    }
}
