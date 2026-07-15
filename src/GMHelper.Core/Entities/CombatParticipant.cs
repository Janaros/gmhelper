using GMHelper.Core.Enums;

namespace GMHelper.Core.Entities;

public class CombatParticipant
{
    public int Id { get; set; }
    public int CombatEncounterId { get; set; }

    /// <summary>Editable short label (e.g. "1d", "a") matching the DM's physical miniature/token.</summary>
    public required string DisplayName { get; set; }

    public CombatParticipantSourceType SourceType { get; set; }
    public int? PlayerId { get; set; }
    public int? MonsterId { get; set; }

    public int? Initiative { get; set; }

    /// <summary>Snapshotted from the "HP" stat field at add-time, so later template edits never
    /// retroactively change a past encounter.</summary>
    public int? CurrentTrackedValue { get; set; }
    public int? MaxTrackedValue { get; set; }

    public string? ConditionsText { get; set; }

    /// <summary>Insertion order — used for display before the encounter starts and as an
    /// initiative-tie tiebreaker afterwards.</summary>
    public int SortOrder { get; set; }

    /// <summary>Soft delete for mid-combat removal, so a removed participant doesn't erase
    /// encounter history.</summary>
    public bool IsActive { get; set; } = true;
}
