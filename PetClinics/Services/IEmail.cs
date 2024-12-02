using PetClinics.Models;

namespace PetClinics.Services
{
    public interface IEmail
    {
        void SendEmail(Email request);
    }
}
