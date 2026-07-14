using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Data;
using Microsoft.EntityFrameworkCore;

namespace GMHelper.Services;

public class PlayerService : IPlayerService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public PlayerService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Player> CreatePlayerAsync(int campaignId, string characterName, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var player = new Player
        {
            CampaignId = campaignId,
            CharacterName = characterName,
        };

        db.Players.Add(player);
        await db.SaveChangesAsync(cancellationToken);

        return player;
    }

    public async Task<IReadOnlyList<Player>> GetPlayersForCampaignAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Players
            .AsNoTracking()
            .Where(p => p.CampaignId == campaignId && p.IsActive)
            .OrderBy(p => p.CharacterName)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdatePlayerAsync(int playerId, string characterName, string? playerName, int? initiative, string? notes, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var player = await db.Players.FindAsync([playerId], cancellationToken);
        if (player is null)
        {
            return;
        }

        player.CharacterName = characterName;
        player.PlayerName = playerName;
        player.Initiative = initiative;
        player.Notes = notes;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePlayerAsync(int playerId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var player = await db.Players.FindAsync([playerId], cancellationToken);
        if (player is null)
        {
            return;
        }

        // Soft delete: keeps the row (and its Id) around for any historical CombatParticipant
        // references added in a later phase, while hiding it from the active roster.
        player.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
    }
}
