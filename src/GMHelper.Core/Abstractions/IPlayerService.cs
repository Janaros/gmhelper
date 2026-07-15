using GMHelper.Core.Entities;

namespace GMHelper.Core.Abstractions;

public interface IPlayerService
{
    Task<Player> CreatePlayerAsync(int campaignId, string characterName, CancellationToken cancellationToken = default);

    /// <summary>Returns every player (active and inactive) so the roster can display and toggle both.</summary>
    Task<IReadOnlyList<Player>> GetPlayersForCampaignAsync(int campaignId, CancellationToken cancellationToken = default);

    Task UpdatePlayerAsync(int playerId, string characterName, string? playerName, int? initiative, string? notes, CancellationToken cancellationToken = default);

    /// <summary>Marks whether the player is currently participating (shown/toggle-able in the roster;
    /// only active players are pulled into a newly prepared combat encounter).</summary>
    Task SetActiveAsync(int playerId, bool isActive, CancellationToken cancellationToken = default);

    Task DeletePlayerAsync(int playerId, CancellationToken cancellationToken = default);
}
