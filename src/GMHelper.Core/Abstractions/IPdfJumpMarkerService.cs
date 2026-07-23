using GMHelper.Core.Entities;

namespace GMHelper.Core.Abstractions;

/// <summary>Manages GM-curated named jump points into a PDF (see <see cref="PdfJumpMarker"/>).</summary>
public interface IPdfJumpMarkerService
{
    Task<PdfJumpMarker> AddJumpMarkerAsync(int pdfDocumentId, string title, int pageNumber, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PdfJumpMarker>> GetJumpMarkersAsync(int pdfDocumentId, CancellationToken cancellationToken = default);

    Task DeleteJumpMarkerAsync(int jumpMarkerId, CancellationToken cancellationToken = default);
}
