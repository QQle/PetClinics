using Microsoft.EntityFrameworkCore;
using PetClinics.Models;

namespace PetClinics.Services
{
    public class PetHelper
    {
        private readonly ClinicDbContext _context;
        public PetHelper(ClinicDbContext clinicDbContext)
        {
            _context = clinicDbContext;
        }
        public async Task<object> GetPetsByUserId(Guid userId)
        {

            var pets = await _context.Pets
                .Where(p => p.PetsOwners.Any(up => up.UserId == userId.ToString()))
                .ToListAsync();
            var petsWithRevaccinationInfo = await NeedRevaccination(pets);
            return new { pets = petsWithRevaccinationInfo }; 
        }

        public async Task<string> GetPetsByBidId(Guid bidId)
        {
            var petName = await _context.Bids
            .Where(b => b.Id == bidId)
            .Select(b => b.Pet.Name)
            .FirstOrDefaultAsync();
            return petName;
        }

        private async Task<List<object>> NeedRevaccination(List<Pet> pets)
        {
            var currentDate = DateTime.Now;
            var vaccinationId = new Guid("CA3AB1E8-9685-4822-81FB-4FC0BA9ED355");
            var result = new List<object>();

            foreach (var pet in pets)
            {
               
                var vaccinationBids = await _context.Bids
                    .Where(b => b.PetId == pet.Id && b.Favors.Id == vaccinationId)
                    .OrderByDescending(b => b.DateOfAdmission)
                    .ToListAsync();

                bool needRevaccination = false;
                if (vaccinationBids.Any())
                {
                    var lastVaccinationDate = vaccinationBids.First().DateOfAdmission;

                    if ((currentDate - lastVaccinationDate).TotalDays > 365)
                    {
                        needRevaccination = true;
                    }
                }

                result.Add(new
                {
                    Id = pet.Id,
                    Name = pet.Name,
                    needRevaccination = needRevaccination
                });
            }

            return result;
        }

    }
}
