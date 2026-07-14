using GMHelper.Core.Entities;

namespace GMHelper.Core.Abstractions;

public interface ICampaignService
{
    Task<Campaign> CreateCampaignAsync(string name, string? description, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Campaign>> GetCampaignsAsync(CancellationToken cancellationToken = default);
    Task DeleteCampaignAsync(int campaignId, CancellationToken cancellationToken = default);
}
