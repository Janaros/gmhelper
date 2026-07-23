using GMHelper.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GMHelper.Data.Configurations;

public class PdfJumpMarkerConfiguration : IEntityTypeConfiguration<PdfJumpMarker>
{
    public void Configure(EntityTypeBuilder<PdfJumpMarker> builder)
    {
        builder.Property(m => m.Title).IsRequired().HasMaxLength(200);

        builder.HasIndex(m => m.PdfDocumentId);

        builder.HasOne<PdfDocument>()
            .WithMany()
            .HasForeignKey(m => m.PdfDocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
