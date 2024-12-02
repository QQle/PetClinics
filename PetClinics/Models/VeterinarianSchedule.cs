namespace PetClinics.Models
{
    public class VeterinarianSchedule
    {
        public Guid Id { get; set; } 
        public Guid VeterinarianId { get; set; } 
        public DateTime AppointmentDate { get; set; } 
        public Veterinarian Veterinarian { get; set; }
    }
}
