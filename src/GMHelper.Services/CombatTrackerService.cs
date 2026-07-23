using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Core.Enums;
using GMHelper.Data;
using Microsoft.EntityFrameworkCore;

namespace GMHelper.Services;

public class CombatTrackerService : ICombatTrackerService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public CombatTrackerService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<CombatEncounter?> GetActiveEncounterAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.CombatEncounters
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.CampaignId == campaignId && e.IsActive, cancellationToken);
    }

    public async Task<CombatEncounter> PrepareEncounterAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var encounter = new CombatEncounter
        {
            CampaignId = campaignId,
            CurrentRound = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        db.CombatEncounters.Add(encounter);
        await db.SaveChangesAsync(cancellationToken);

        var activePlayers = await db.Players
            .Where(p => p.CampaignId == campaignId && p.IsActive)
            .OrderBy(p => p.CharacterName)
            .ToListAsync(cancellationToken);

        var sortOrder = 0;
        foreach (var player in activePlayers)
        {
            var hp = await TryGetIntStatFieldAsync(db, StatFieldOwnerType.Player, player.Id, "HP", cancellationToken);
            var armorClass = await TryGetIntStatFieldAsync(db, StatFieldOwnerType.Player, player.Id, "RK", cancellationToken);
            var tokenNumber = await TryGetStatFieldAsync(db, StatFieldOwnerType.Player, player.Id, "TK", cancellationToken);

            db.CombatParticipants.Add(new CombatParticipant
            {
                CombatEncounterId = encounter.Id,
                DisplayName = player.CharacterName,
                SourceType = CombatParticipantSourceType.PlayerRef,
                PlayerId = player.Id,
                Initiative = player.Initiative,
                CurrentTrackedValue = hp,
                MaxTrackedValue = hp,
                ArmorClass = armorClass,
                TokenNumber = tokenNumber,
                SortOrder = sortOrder++,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return encounter;
    }

    public async Task<IReadOnlyList<CombatParticipant>> GetParticipantsAsync(int combatEncounterId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.CombatParticipants
            .AsNoTracking()
            .Where(p => p.CombatEncounterId == combatEncounterId && p.IsActive)
            .OrderByDescending(p => p.Initiative)
            .ThenBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<CombatParticipant> AddMonsterParticipantAsync(int combatEncounterId, int monsterId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var monster = await db.Monsters.FindAsync([monsterId], cancellationToken)
            ?? throw new InvalidOperationException($"Monster {monsterId} not found.");

        var existingOfSameMonster = await db.CombatParticipants
            .CountAsync(p => p.CombatEncounterId == combatEncounterId && p.MonsterId == monsterId, cancellationToken);

        var maxSortOrder = await db.CombatParticipants
            .Where(p => p.CombatEncounterId == combatEncounterId)
            .Select(p => (int?)p.SortOrder)
            .MaxAsync(cancellationToken) ?? -1;

        var hp = await TryGetIntStatFieldAsync(db, StatFieldOwnerType.Monster, monsterId, "HP", cancellationToken);
        var armorClass = await TryGetIntStatFieldAsync(db, StatFieldOwnerType.Monster, monsterId, "RK", cancellationToken);
        var tokenNumber = await TryGetStatFieldAsync(db, StatFieldOwnerType.Monster, monsterId, "TK", cancellationToken);

        var participant = new CombatParticipant
        {
            CombatEncounterId = combatEncounterId,
            DisplayName = $"{monster.Name} {existingOfSameMonster + 1}",
            SourceType = CombatParticipantSourceType.MonsterInstance,
            MonsterId = monsterId,
            CurrentTrackedValue = hp,
            MaxTrackedValue = hp,
            ArmorClass = armorClass,
            TokenNumber = tokenNumber,
            SortOrder = maxSortOrder + 1,
        };

        db.CombatParticipants.Add(participant);
        await db.SaveChangesAsync(cancellationToken);

        return participant;
    }

    public async Task UpdateParticipantAsync(
        int participantId,
        string displayName,
        int? initiative,
        int? currentTrackedValue,
        string? conditionsText,
        int? armorClass = null,
        string? tokenNumber = null,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var participant = await db.CombatParticipants.FindAsync([participantId], cancellationToken);
        if (participant is null)
        {
            return;
        }

        participant.DisplayName = displayName;
        participant.Initiative = initiative;
        participant.CurrentTrackedValue = currentTrackedValue;
        participant.ConditionsText = conditionsText;
        participant.ArmorClass = armorClass;
        participant.TokenNumber = tokenNumber;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveParticipantAsync(int participantId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var participant = await db.CombatParticipants.FindAsync([participantId], cancellationToken);
        if (participant is null)
        {
            return;
        }

        participant.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task StartEncounterAsync(int combatEncounterId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var encounter = await db.CombatEncounters.FindAsync([combatEncounterId], cancellationToken);
        if (encounter is null)
        {
            return;
        }

        var firstParticipant = await db.CombatParticipants
            .Where(p => p.CombatEncounterId == combatEncounterId && p.IsActive)
            .OrderByDescending(p => p.Initiative)
            .ThenBy(p => p.SortOrder)
            .FirstOrDefaultAsync(cancellationToken);

        encounter.CurrentRound = 1;
        encounter.CurrentTurnParticipantId = firstParticipant?.Id;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AdvanceTurnAsync(int combatEncounterId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var encounter = await db.CombatEncounters.FindAsync([combatEncounterId], cancellationToken);
        if (encounter is null)
        {
            return;
        }

        var orderedParticipants = await db.CombatParticipants
            .Where(p => p.CombatEncounterId == combatEncounterId && p.IsActive)
            .OrderByDescending(p => p.Initiative)
            .ThenBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);

        if (orderedParticipants.Count == 0)
        {
            return;
        }

        var currentIndex = orderedParticipants.FindIndex(p => p.Id == encounter.CurrentTurnParticipantId);
        var nextIndex = currentIndex + 1;

        if (nextIndex >= orderedParticipants.Count)
        {
            nextIndex = 0;
            encounter.CurrentRound++;
        }

        encounter.CurrentTurnParticipantId = orderedParticipants[nextIndex].Id;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task EndEncounterAsync(int combatEncounterId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var encounter = await db.CombatEncounters.FindAsync([combatEncounterId], cancellationToken);
        if (encounter is null)
        {
            return;
        }

        encounter.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Looks up a stat field by exact name (e.g. "HP", "RK", "TK"), so a participant's
    /// snapshot values can be pre-filled from the Player/Monster template on add.</summary>
    private static async Task<string?> TryGetStatFieldAsync(AppDbContext db, StatFieldOwnerType ownerType, int ownerId, string fieldName, CancellationToken cancellationToken)
    {
        var statField = await db.StatFields
            .Where(s => s.OwnerType == ownerType && s.OwnerId == ownerId && s.Name == fieldName)
            .FirstOrDefaultAsync(cancellationToken);

        return statField?.Value;
    }

    private static async Task<int?> TryGetIntStatFieldAsync(AppDbContext db, StatFieldOwnerType ownerType, int ownerId, string fieldName, CancellationToken cancellationToken)
    {
        var value = await TryGetStatFieldAsync(db, ownerType, ownerId, fieldName, cancellationToken);
        return value is not null && int.TryParse(value, out var parsed) ? parsed : null;
    }
}
