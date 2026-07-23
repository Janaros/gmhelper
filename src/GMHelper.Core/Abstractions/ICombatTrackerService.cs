using GMHelper.Core.Entities;

namespace GMHelper.Core.Abstractions;

/// <summary>
/// Owns all combat state transitions so the ViewModel stays a thin UI-orchestration layer.
/// </summary>
public interface ICombatTrackerService
{
    Task<CombatEncounter?> GetActiveEncounterAsync(int campaignId, CancellationToken cancellationToken = default);

    /// <summary>Creates a new encounter and immediately adds every active player from the roster.</summary>
    Task<CombatEncounter> PrepareEncounterAsync(int campaignId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CombatParticipant>> GetParticipantsAsync(int combatEncounterId, CancellationToken cancellationToken = default);

    Task<CombatParticipant> AddMonsterParticipantAsync(int combatEncounterId, int monsterId, CancellationToken cancellationToken = default);

    Task UpdateParticipantAsync(
        int participantId,
        string displayName,
        int? initiative,
        int? currentTrackedValue,
        string? conditionsText,
        int? armorClass = null,
        string? tokenNumber = null,
        CancellationToken cancellationToken = default);

    Task RemoveParticipantAsync(int participantId, CancellationToken cancellationToken = default);

    /// <summary>Sorts participants by initiative (descending) and begins round 1.</summary>
    Task StartEncounterAsync(int combatEncounterId, CancellationToken cancellationToken = default);

    /// <summary>Moves to the next active participant in initiative order, incrementing the round on wraparound.</summary>
    Task AdvanceTurnAsync(int combatEncounterId, CancellationToken cancellationToken = default);

    /// <summary>Archives the encounter (IsActive = false) without deleting it.</summary>
    Task EndEncounterAsync(int combatEncounterId, CancellationToken cancellationToken = default);
}
