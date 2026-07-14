using GMHelper.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GMHelper.Data.Configurations;

public class ImageAssetConfiguration : IEntityTypeConfiguration<ImageAsset>
{
    public void Configure(EntityTypeBuilder<ImageAsset> builder)
    {
        builder.Property(i => i.FileName).IsRequired().HasMaxLength(260);
        builder.Property(i => i.StoredRelativePath).IsRequired().HasMaxLength(1000);

        builder.HasIndex(i => new { i.OwnerType, i.OwnerId });
    }
}
