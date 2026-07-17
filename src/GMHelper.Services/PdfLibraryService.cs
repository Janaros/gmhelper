using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Data;
using Microsoft.EntityFrameworkCore;

namespace GMHelper.Services;

public class PdfLibraryService : IPdfLibraryService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IAppPaths _appPaths;

    public PdfLibraryService(IDbContextFactory<AppDbContext> dbContextFactory, IAppPaths appPaths)
    {
        _dbContextFactory = dbContextFactory;
        _appPaths = appPaths;
    }

    public async Task<PdfDocument> AddPdfToCampaignAsync(int campaignId, string sourceFilePath, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var pdfsFolder = _appPaths.CampaignPdfsFolder(campaignId);
        Directory.CreateDirectory(pdfsFolder);

        var destinationPath = FileNaming.ResolveUniqueDestinationPath(pdfsFolder, Path.GetFileName(sourceFilePath));
        File.Copy(sourceFilePath, destinationPath);

        var maxDisplayOrder = await db.PdfDocuments
            .Where(p => p.CampaignId == campaignId)
            .Select(p => (int?)p.DisplayOrder)
            .MaxAsync(cancellationToken) ?? -1;

        var pdfDocument = new PdfDocument
        {
            CampaignId = campaignId,
            FileName = Path.GetFileName(destinationPath),
            StoredRelativePath = Path.GetRelativePath(_appPaths.DataRoot, destinationPath),
            DisplayOrder = maxDisplayOrder + 1,
            LastViewedPage = 1,
            AddedAt = DateTime.UtcNow,
        };

        db.PdfDocuments.Add(pdfDocument);
        await db.SaveChangesAsync(cancellationToken);

        return pdfDocument;
    }

    public async Task<IReadOnlyList<PdfDocument>> GetPdfsForCampaignAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.PdfDocuments
            .AsNoTracking()
            .Where(p => p.CampaignId == campaignId)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateLastViewedPageAsync(int pdfDocumentId, int pageNumber, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var pdfDocument = await db.PdfDocuments.FindAsync([pdfDocumentId], cancellationToken);
        if (pdfDocument is null)
        {
            return;
        }

        pdfDocument.LastViewedPage = pageNumber;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePdfAsync(int pdfDocumentId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var pdfDocument = await db.PdfDocuments.FindAsync([pdfDocumentId], cancellationToken);
        if (pdfDocument is null)
        {
            return;
        }

        var filePath = GetAbsoluteFilePath(pdfDocument);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        var backupPath = filePath + ".bak";
        if (File.Exists(backupPath))
        {
            File.Delete(backupPath);
        }

        db.PdfDocuments.Remove(pdfDocument);
        await db.SaveChangesAsync(cancellationToken);
    }

    public string GetAbsoluteFilePath(PdfDocument pdfDocument) =>
        Path.Combine(_appPaths.DataRoot, pdfDocument.StoredRelativePath);

    public Task CreateBackupAsync(PdfDocument pdfDocument, CancellationToken cancellationToken = default)
    {
        var filePath = GetAbsoluteFilePath(pdfDocument);
        var backupPath = filePath + ".bak";
        File.Copy(filePath, backupPath, overwrite: true);
        return Task.CompletedTask;
    }
}
