namespace PetClinics.Models
{
    public class Bids
    {
        public Guid? Id { get; set; }
        public string? UserId { get; set; }
        public Guid? PetId { get; set; }
        public Guid? VeterinarianId {  get; set; }
        public Guid? FavorsId { get; set; }
        public Favors? Favors { get; set; }
        public Veterinarian? Veterinarian {  get; set; }
        public ExtendedUser? User { get; set; }
        public Pet? Pet { get; set; }
    }
}
