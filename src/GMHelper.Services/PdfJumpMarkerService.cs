using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Data;
using Microsoft.EntityFrameworkCore;

namespace GMHelper.Services;

public class PdfJumpMarkerService : IPdfJumpMarkerService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public PdfJumpMarkerService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<PdfJumpMarker> AddJumpMarkerAsync(int pdfDocumentId, string title, int pageNumber, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var marker = new PdfJumpMarker
        {
            PdfDocumentId = pdfDocumentId,
            Title = title,
            PageNumber = pageNumber,
            CreatedAt = DateTime.UtcNow,
        };

        db.PdfJumpMarkers.Add(marker);
        await db.SaveChangesAsync(cancellationToken);

        return marker;
    }

    public async Task<IReadOnlyList<PdfJumpMarker>> GetJumpMarkersAsync(int pdfDocumentId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.PdfJumpMarkers
            .AsNoTracking()
            .Where(m => m.PdfDocumentId == pdfDocumentId)
            .OrderBy(m => m.PageNumber)
            .ThenBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteJumpMarkerAsync(int jumpMarkerId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var marker = await db.PdfJumpMarkers.FindAsync([jumpMarkerId], cancellationToken);
        if (marker is null)
        {
            return;
        }

        db.PdfJumpMarkers.Remove(marker);
        await db.SaveChangesAsync(cancellationToken);
    }
}
