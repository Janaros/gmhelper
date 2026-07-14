using GMHelper.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GMHelper.Data.Configurations;

public class MonsterConfiguration : IEntityTypeConfiguration<Monster>
{
    public void Configure(EntityTypeBuilder<Monster> builder)
    {
        builder.Property(m => m.Name).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Notes).HasMaxLength(4000);
        builder.Property(m => m.Source).IsRequired().HasMaxLength(200);

        builder.HasIndex(m => m.Name);
    }
}
