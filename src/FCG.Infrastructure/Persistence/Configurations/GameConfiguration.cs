using FCG.Domain.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Infrastructure.Persistence.Configurations;

internal sealed class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("games");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasColumnName("id");

        builder.Property(g => g.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(g => g.NormalizedName)
            .HasColumnName("normalized_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(g => g.NormalizedName).IsUnique();

        builder.Property(g => g.Description)
            .HasColumnName("description")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(g => g.BasePrice)
            .HasColumnName("base_price")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(g => g.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
    }
}
