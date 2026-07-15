namespace GMHelper.Core.Abstractions;

public interface ICampaignExportService
{
    /// <summary>
    /// Writes a self-contained backup zip: a JSON dump of the campaign's data rows plus its
    /// entire PDFs/Images folder tree. Not a re-importable format — a GM safety-net backup.
    /// </summary>
    Task ExportCampaignAsync(int campaignId, string destinationZipFilePath, CancellationToken cancellationToken = default);
}
