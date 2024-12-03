using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinics.Models;

namespace PetClinics.Services
{
    public class UserHelper 
    {
        private readonly ClinicDbContext _context;
        private readonly string VeterinarianRole = "Veterinarian";
        private readonly string Success = "Success";
        private readonly string ValidationError = "User not found";
        public UserHelper( ClinicDbContext context)
        {
            _context = context;
        }
        public async Task<Guid> GetUserIdByUserName(string username)
        {
            try
            {
                return await _context.Users
                    .Where(x => x.UserName == username)
                    .Select(x => Guid.Parse(x.Id))
                    .FirstOrDefaultAsync(); 
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при получении идентификатора пользователя", ex);
            }

        }
        public async Task<string> GetUserNameByUserId(Guid userId)
        {
            try
            {
                var userName =  await _context.Users
                    .Where(x => x.Id == userId.ToString())
                    .Select(x => x.UserName)
                    .FirstOrDefaultAsync();
                return userName;
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при получении идентификатора пользователя", ex);
            }

        }
        public async Task<string> GetUserEmailById(Guid userId)
        {
            try
            {
                var email =  await _context.Users
                    .Where(u => u.Id == userId.ToString())
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();
                return email;
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при получении почты пользователя", ex);
            }
        }
        public async Task<string> ValidateUser(Guid userId)
        {
            try
            {
                var currentUser = await _context.Users
                    .Where(u => u.Id == userId.ToString())
                    .FirstOrDefaultAsync();
                if (currentUser == null)
                    throw new Exception(ValidationError);

                return Success;
            }
            catch (Exception ex)
            {
                return $"Ошибка валидации: {ex.Message}";
            }
        }
        public async Task<bool> CheckForExtendedUserInfo(Guid userId, bool hasVeterinarianRole)
        {
            if (!hasVeterinarianRole)
            {
                return false;
            }

            var veterinarian = await _context.Veterinarians
                    .FirstOrDefaultAsync(v => v.Id == userId);

            if (veterinarian == null)
            {

                return false;
            }
            if (string.IsNullOrWhiteSpace(veterinarian.Specialization) ||
                string.IsNullOrWhiteSpace(veterinarian.Address) ||
                veterinarian.YearsOfExperience <= 0 ||
                veterinarian.Price <= 0)
            {
                return false;
            }
            return true;
        }
        public async Task<object> GetUserRole(Guid userId)
        {
            var role = await _context.UserRoles
                .Where(ur =>ur.UserId == userId.ToString())
                .Join(
                    _context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r.Name
                )
                .FirstOrDefaultAsync();
            return role;
        }
        public async Task<bool> HasVeterinarionRole(Guid userId)
        {
            var userRole = await GetUserRole(userId);
            if (userRole.ToString() != VeterinarianRole)
            { 
                return false;
            }
            return true;

        }

        public async Task<bool> SetIsAccepted(Guid bidId)
        {
            var currentBid = await _context.Bids.FirstOrDefaultAsync(x => x.Id == bidId);
            if (currentBid == null)
            {
                return false;
            }

            currentBid.IsAccepted = true;

            _context.Bids.Update(currentBid);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<object> GetUserBids(Guid userId)
        {
            try
            {
                var userBids = await _context.Bids
                    .Where(b => b.UserId == userId.ToString())
                    .Include(b => b.Pet) 
                    .Include(b => b.Veterinarian)
                    .Include(b => b.Favors) 
                    .Select(b => new
                    {
                        PetName = b.Pet.Name, 
                        VeterinarianName = b.Veterinarian.Name,
                        ServiceName = b.Favors.Name, 
                        DateOfAdmission = b.DateOfAdmission
                    })
                    .ToListAsync();

                if (!userBids.Any())
                {
                    return new { Message = "У пользователя нет заявок" };
                }

                return userBids;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении заявок: {ex.Message}");
                return new { Message = "Произошла ошибка при обработке запроса" };
            }
        }  

    }
}
