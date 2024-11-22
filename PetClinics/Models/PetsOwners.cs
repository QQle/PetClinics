namespace PetClinics.Models
{
    /// <summary>
    /// Класс, представляющий связь между владельцем и питомцем.
    /// </summary>
    public class PetsOwners
    {
        public Guid Id { get; set; } 
        public string? UserId { get; set; } 
        public Guid PetId { get; set; }
        public ExtendedUser? User { get; set; }
        public Pet? Pet { get; set; }
    }
}
