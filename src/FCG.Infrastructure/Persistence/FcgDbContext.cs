using FCG.Domain.Games;
using FCG.Domain.Libraries;
using FCG.Domain.Promotions;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FCG.Infrastructure.Persistence;

public sealed class FcgDbContext(DbContextOptions<FcgDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<LibraryItem> LibraryItems => Set<LibraryItem>();
    public DbSet<Promotion> Promotions => Set<Promotion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var emailConverter = new ValueConverter<Email, string>(email => email.Value, value => Email.Create(value));
        modelBuilder.Entity<User>(entity => { entity.ToTable("users"); entity.HasKey(x => x.Id); entity.Property(x => x.Name).HasMaxLength(150); entity.Property(x => x.Email).HasConversion(emailConverter).HasMaxLength(254); entity.HasIndex(x => x.Email).IsUnique(); entity.Property(x => x.PasswordHash).HasMaxLength(500); entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(32); });
        modelBuilder.Entity<Game>(entity => { entity.ToTable("games"); entity.HasKey(x => x.Id); entity.Property(x => x.Name).HasMaxLength(200); entity.Property(x => x.NormalizedName).HasMaxLength(200); entity.HasIndex(x => x.NormalizedName).IsUnique(); entity.Property(x => x.Description).HasMaxLength(2000); entity.Property(x => x.BasePrice).HasPrecision(18, 2); });
        modelBuilder.Entity<LibraryItem>(entity => { entity.ToTable("library_items"); entity.HasKey(x => x.Id); entity.Property(x => x.PricePaid).HasPrecision(18, 2); entity.HasIndex(x => new { x.PlayerId, x.GameId }).IsUnique(); });
        modelBuilder.Entity<Promotion>(entity => { entity.ToTable("promotions"); entity.HasKey(x => x.Id); entity.Property(x => x.DiscountPercentage).HasPrecision(5, 2); entity.HasIndex(x => x.GameId); });
    }
}
