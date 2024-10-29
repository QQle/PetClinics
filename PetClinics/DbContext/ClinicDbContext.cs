using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetClinics.Models;
public class ClinicDbContext : IdentityDbContext
{
        public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Veterinarian> Veterinarians { get; set; }
        public DbSet<ExtendedUser> ExtendedUser { get; set; }
        public DbSet<Favors> Favors { get; set; }
        public DbSet<Bids> Bids { get; set; }
      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PetsOwners>()
                .HasKey(up => up.Id);

            modelBuilder.Entity<PetsOwners>()
                .HasOne(up => up.User)
                .WithMany(u => u.PetsOwners)
                .HasForeignKey(up => up.UserId);

            modelBuilder.Entity<PetsOwners>()
                .HasOne(up => up.Pet)
                .WithMany(p => p.PetsOwners)
                .HasForeignKey(up => up.PetId);

            modelBuilder.Entity<Bids>()
               .HasOne(b => b.User)
               .WithMany(u => u.Bids)
               .HasForeignKey(b => b.UserId)
               .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<Bids>()
                .HasOne(b => b.Pet)
                .WithMany(p => p.Bids)
                .HasForeignKey(b => b.PetId)
                .OnDelete(DeleteBehavior.SetNull); 

            modelBuilder.Entity<Bids>()
                .HasOne(b => b.Veterinarian)
                .WithMany(v => v.Bids)
                .HasForeignKey(b => b.VeterinarianId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Bids>()
                .HasOne(b => b.Favors)
                .WithMany(f => f.Bids)
                .HasForeignKey(b => b.FavorsId)
                .OnDelete(DeleteBehavior.SetNull);

      }
}




