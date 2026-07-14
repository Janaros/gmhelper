using GMHelper.Core.Entities;

namespace GMHelper.Core.Abstractions;

public interface IPlayerService
{
    Task<Player> CreatePlayerAsync(int campaignId, string characterName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Player>> GetPlayersForCampaignAsync(int campaignId, CancellationToken cancellationToken = default);
    Task UpdatePlayerAsync(int playerId, string characterName, string? playerName, int? initiative, string? notes, CancellationToken cancellationToken = default);
    Task DeletePlayerAsync(int playerId, CancellationToken cancellationToken = default);
}
