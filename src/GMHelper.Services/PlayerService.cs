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
            .Where(p => p.CampaignId == campaignId)
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

    public async Task SetActiveAsync(int playerId, bool isActive, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var player = await db.Players.FindAsync([playerId], cancellationToken);
        if (player is null)
        {
            return;
        }

        player.IsActive = isActive;
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

        db.Players.Remove(player);
        await db.SaveChangesAsync(cancellationToken);
    }
}
