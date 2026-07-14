namespace GMHelper.Core.Entities;

/// <summary>
/// Global, campaign-independent monster template. Combat instances (added in a later phase)
/// reference this by Id and snapshot their own HP/state, so editing a template here never
/// retroactively changes a past encounter.
/// </summary>
public class Monster
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int? ImageAssetId { get; set; }
    public string? Notes { get; set; }
    public required string Source { get; set; }
    public DateTime CreatedAt { get; set; }
}
