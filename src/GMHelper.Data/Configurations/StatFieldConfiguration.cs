using GMHelper.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GMHelper.Data.Configurations;

public class StatFieldConfiguration : IEntityTypeConfiguration<StatField>
{
    public void Configure(EntityTypeBuilder<StatField> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Value).IsRequired().HasMaxLength(2000);

        builder.HasIndex(s => new { s.OwnerType, s.OwnerId, s.SortOrder });
    }
}
