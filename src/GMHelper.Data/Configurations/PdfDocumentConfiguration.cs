using GMHelper.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GMHelper.Data.Configurations;

public class PdfDocumentConfiguration : IEntityTypeConfiguration<PdfDocument>
{
    public void Configure(EntityTypeBuilder<PdfDocument> builder)
    {
        builder.Property(p => p.FileName).IsRequired().HasMaxLength(260);
        builder.Property(p => p.StoredRelativePath).IsRequired().HasMaxLength(1000);

        builder.HasIndex(p => p.CampaignId);

        builder.HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(p => p.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
