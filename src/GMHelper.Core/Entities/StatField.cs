using GMHelper.Core.Enums;

namespace GMHelper.Core.Entities;

/// <summary>
/// A single named value on a Player or Monster (e.g. "AC" = "15"). Deliberately free-form
/// (string name/value) rather than a fixed schema, since different tabletop systems track
/// different stats. Initiative is NOT stored here — it is a first-class column on Player and
/// CombatParticipant because the combat tracker must sort on it without a join.
/// </summary>
public class StatField
{
    public int Id { get; set; }
    public StatFieldOwnerType OwnerType { get; set; }
    public int OwnerId { get; set; }
    public required string Name { get; set; }
    public required string Value { get; set; }
    public int SortOrder { get; set; }
}
