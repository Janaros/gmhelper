using GMHelper.Core.Entities;

namespace GMHelper.Core.Abstractions;

public interface IPdfLibraryService
{
    Task<PdfDocument> AddPdfToCampaignAsync(int campaignId, string sourceFilePath, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PdfDocument>> GetPdfsForCampaignAsync(int campaignId, CancellationToken cancellationToken = default);
    Task UpdateLastViewedPageAsync(int pdfDocumentId, int pageNumber, CancellationToken cancellationToken = default);
    string GetAbsoluteFilePath(PdfDocument pdfDocument);

    /// <summary>
    /// Copies the PDF's current file to a sibling ".bak" file before an in-place save,
    /// so a corrupted/interrupted write from the viewer control never destroys the only copy.
    /// </summary>
    Task CreateBackupAsync(PdfDocument pdfDocument, CancellationToken cancellationToken = default);
}
