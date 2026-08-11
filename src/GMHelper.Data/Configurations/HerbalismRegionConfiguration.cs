using GMHelper.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GMHelper.Data.Configurations;

public class HerbalismRegionConfiguration : IEntityTypeConfiguration<HerbalismRegion>
{
    public void Configure(EntityTypeBuilder<HerbalismRegion> builder)
    {
        builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Terrain).HasMaxLength(200);
        builder.Property(r => r.Description).HasMaxLength(4000);
        builder.Property(r => r.Source).IsRequired().HasMaxLength(200);

        builder.HasIndex(r => r.Name);
    }
}
