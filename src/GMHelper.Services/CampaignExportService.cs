using System.IO.Compression;
using System.Text.Json;
using GMHelper.Core.Abstractions;
using GMHelper.Core.Enums;
using GMHelper.Core.Models;
using GMHelper.Data;
using Microsoft.EntityFrameworkCore;

namespace GMHelper.Services;

public class CampaignExportService : ICampaignExportService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IAppPaths _appPaths;

    public CampaignExportService(IDbContextFactory<AppDbContext> dbContextFactory, IAppPaths appPaths)
    {
        _dbContextFactory = dbContextFactory;
        _appPaths = appPaths;
    }

    public async Task ExportCampaignAsync(int campaignId, string destinationZipFilePath, CancellationToken cancellationToken = default)
    {
        var exportData = await BuildExportDataAsync(campaignId, cancellationToken);
        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });

        if (File.Exists(destinationZipFilePath))
        {
            File.Delete(destinationZipFilePath);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(destinationZipFilePath) ?? ".");

        using var zipStream = new FileStream(destinationZipFilePath, FileMode.Create);
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);

        var jsonEntry = archive.CreateEntry("campaign-data.json");
        await using (var entryStream = jsonEntry.Open())
        await using (var writer = new StreamWriter(entryStream))
        {
            await writer.WriteAsync(json);
        }

        var campaignFolder = _appPaths.CampaignFolder(campaignId);
        if (Directory.Exists(campaignFolder))
        {
            foreach (var file in Directory.EnumerateFiles(campaignFolder, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(campaignFolder, file);
                var entryName = "Files/" + relativePath.Replace('\\', '/');
                archive.CreateEntryFromFile(file, entryName);
            }
        }
    }

    private async Task<CampaignExportData> BuildExportDataAsync(int campaignId, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var campaign = await db.Campaigns.AsNoTracking().FirstOrDefaultAsync(c => c.Id == campaignId, cancellationToken)
            ?? throw new InvalidOperationException($"Campaign {campaignId} not found.");

        var pdfDocuments = await db.PdfDocuments.AsNoTracking()
            .Where(p => p.CampaignId == campaignId)
            .ToListAsync(cancellationToken);

        var imageAssets = await db.ImageAssets.AsNoTracking()
            .Where(i => i.OwnerType == ImageOwnerType.Campaign && i.OwnerId == campaignId)
            .ToListAsync(cancellationToken);

        var players = await db.Players.AsNoTracking()
            .Where(p => p.CampaignId == campaignId)
            .ToListAsync(cancellationToken);
        var playerIds = players.Select(p => p.Id).ToList();

        var playerStatFields = await db.StatFields.AsNoTracking()
            .Where(s => s.OwnerType == StatFieldOwnerType.Player && playerIds.Contains(s.OwnerId))
            .ToListAsync(cancellationToken);

        var sessionNotes = await db.SessionNotes.AsNoTracking()
            .Where(n => n.CampaignId == campaignId)
            .ToListAsync(cancellationToken);

        var combatEncounters = await db.CombatEncounters.AsNoTracking()
            .Where(e => e.CampaignId == campaignId)
            .ToListAsync(cancellationToken);
        var encounterIds = combatEncounters.Select(e => e.Id).ToList();

        var combatParticipants = await db.CombatParticipants.AsNoTracking()
            .Where(p => encounterIds.Contains(p.CombatEncounterId))
            .ToListAsync(cancellationToken);

        return new CampaignExportData
        {
            Campaign = campaign,
            PdfDocuments = pdfDocuments,
            ImageAssets = imageAssets,
            Players = players,
            PlayerStatFields = playerStatFields,
            SessionNotes = sessionNotes,
            CombatEncounters = combatEncounters,
            CombatParticipants = combatParticipants,
        };
    }
}
