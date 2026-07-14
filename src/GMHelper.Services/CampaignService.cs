using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Data;
using Microsoft.EntityFrameworkCore;

namespace GMHelper.Services;

public class CampaignService : ICampaignService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IAppPaths _appPaths;

    public CampaignService(IDbContextFactory<AppDbContext> dbContextFactory, IAppPaths appPaths)
    {
        _dbContextFactory = dbContextFactory;
        _appPaths = appPaths;
    }

    public async Task<Campaign> CreateCampaignAsync(string name, string? description, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var campaign = new Campaign
        {
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
        };

        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync(cancellationToken);

        Directory.CreateDirectory(_appPaths.CampaignPdfsFolder(campaign.Id));
        Directory.CreateDirectory(_appPaths.CampaignImagesFolder(campaign.Id));

        return campaign;
    }

    public async Task<IReadOnlyList<Campaign>> GetCampaignsAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Campaigns
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteCampaignAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var campaign = await db.Campaigns.FindAsync([campaignId], cancellationToken);
        if (campaign is null)
        {
            return;
        }

        db.Campaigns.Remove(campaign);
        await db.SaveChangesAsync(cancellationToken);

        var folder = _appPaths.CampaignFolder(campaignId);
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}
