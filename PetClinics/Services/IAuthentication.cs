using PetClinics.Models;

namespace PetClinics.Services
{
    public interface IAuthentication
    {
        Task<bool> Registration(RegistrationUser user);
        Task<AuthorizationResponse> Login(LoginUser user);
        Task<AuthorizationResponse> RefreshToken(RefreshToken model);
    }
}
