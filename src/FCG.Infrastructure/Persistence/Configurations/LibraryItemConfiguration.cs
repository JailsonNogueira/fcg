using FCG.Domain.Libraries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Infrastructure.Persistence.Configurations;

internal sealed class LibraryItemConfiguration : IEntityTypeConfiguration<LibraryItem>
{
    public void Configure(EntityTypeBuilder<LibraryItem> builder)
    {
        builder.ToTable("library_items");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");

        builder.Property(l => l.PlayerId)
            .HasColumnName("player_id")
            .IsRequired();

        builder.Property(l => l.GameId)
            .HasColumnName("game_id")
            .IsRequired();

        builder.HasIndex(l => new { l.PlayerId, l.GameId }).IsUnique();

        builder.Property(l => l.AcquiredAt)
            .HasColumnName("acquired_at")
            .IsRequired();

        builder.Property(l => l.PricePaid)
            .HasColumnName("price_paid")
            .HasColumnType("numeric(18,2)")
            .IsRequired();
    }
}
