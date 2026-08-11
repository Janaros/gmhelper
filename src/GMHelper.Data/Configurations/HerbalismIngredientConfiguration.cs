using GMHelper.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GMHelper.Data.Configurations;

public class HerbalismIngredientConfiguration : IEntityTypeConfiguration<HerbalismIngredient>
{
    public void Configure(EntityTypeBuilder<HerbalismIngredient> builder)
    {
        builder.Property(i => i.Name).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Effect).HasMaxLength(4000);
        builder.Property(i => i.Notes).HasMaxLength(4000);

        builder.HasIndex(i => new { i.HerbalismRegionId, i.Name });

        // Kaskade, weil eine Zutat ohne ihr Gebiet keine Bedeutung hat: die Fundtabelle
        // existiert nur als Teil des Gebiets.
        builder.HasOne<HerbalismRegion>()
            .WithMany()
            .HasForeignKey(i => i.HerbalismRegionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
