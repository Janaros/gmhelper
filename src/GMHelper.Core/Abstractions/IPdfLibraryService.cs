using GMHelper.Core.Entities;

namespace GMHelper.Core.Abstractions;

public interface IPdfLibraryService
{
    Task<PdfDocument> AddPdfToCampaignAsync(int campaignId, string sourceFilePath, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PdfDocument>> GetPdfsForCampaignAsync(int campaignId, CancellationToken cancellationToken = default);
    Task UpdateLastViewedPageAsync(int pdfDocumentId, int pageNumber, CancellationToken cancellationToken = default);
    string GetAbsoluteFilePath(PdfDocument pdfDocument);
}
