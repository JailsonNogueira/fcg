using FCG.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Infrastructure.Persistence.Configurations;

internal sealed class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("promotions");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.GameId)
            .HasColumnName("game_id")
            .IsRequired();

        builder.Property(p => p.DiscountPercentage)
            .HasColumnName("discount_percentage")
            .HasColumnType("numeric(5,2)")
            .IsRequired();

        builder.Property(p => p.StartsAt)
            .HasColumnName("starts_at")
            .IsRequired();

        builder.Property(p => p.EndsAt)
            .HasColumnName("ends_at")
            .IsRequired();

        builder.Property(p => p.IsEnabled)
            .HasColumnName("is_enabled")
            .IsRequired();
    }
}
