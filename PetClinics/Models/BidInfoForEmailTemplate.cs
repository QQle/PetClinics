namespace PetClinics.Models
{
    public class BidInfoForEmailTemplate
    {
        public Guid UserId { get; set; }
        public DateTime DateOfAdmission { get; set; }
        public string FavorName { get; set; } = "";
        public double FavorPrice { get; set; }
        public string VeterinarianName { get; set; } = "";
        public double VeterinarianPrice { get; set; }
        public string VeterinarianPhoto { get; set; } = "";
    }
}
