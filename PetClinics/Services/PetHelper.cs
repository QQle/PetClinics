using Microsoft.EntityFrameworkCore;

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
            return pets;
        }

        public async Task<string> GetPetsByBidId(Guid bidId)
        {
            var petName = await _context.Bids
            .Where(b => b.Id == bidId)
            .Select(b => b.Pet.Name)
            .FirstOrDefaultAsync();
            return petName;
        }

    }
}
