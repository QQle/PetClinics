using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetClinics.Models;

/// <summary>
/// Контекст базы данных для работы с сущностями клиники, наследуемый от IdentityDbContext.
/// </summary>
public class ClinicDbContext : IdentityDbContext
{
    /// <summary>
    /// Конструктор контекста базы данных.
    /// </summary>
    /// <param name="options">Параметры конфигурации контекста.</param>
    public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options)
    {
            Database.EnsureCreated();
    }
    /// <summary>
    /// Таблица с питомцами.
    /// </summary>
    public DbSet<Pet> Pets { get; set; }

    /// <summary>
    /// Таблица с ветеринарами.
    /// </summary>
    public DbSet<Veterinarian> Veterinarians { get; set; }

    /// <summary>
    /// Таблица с пользователями.
    /// </summary>
    public DbSet<ExtendedUser> ExtendedUser { get; set; }

    /// <summary>
    /// Таблица с услугами.
    /// </summary>
    public DbSet<Favors> Favors { get; set; }

    /// <summary>
    /// Таблица с заявками.
    /// </summary>
    public DbSet<Bids> Bids { get; set; }

    /// <summary>
    /// Таблица для связи пользователей с питомцами.
    /// </summary>
    public DbSet<PetsOwners> PetsOwners { get; set; }

    /// <summary>
    /// Метод для конфигурации моделей базы данных.
    /// </summary>
    /// <param name="modelBuilder">Объект ModelBuilder для настройки сущностей.</param>
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




