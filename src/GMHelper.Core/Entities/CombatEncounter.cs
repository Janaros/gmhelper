namespace GMHelper.Core.Entities;

public class CombatEncounter
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public int CurrentRound { get; set; }
    public int? CurrentTurnParticipantId { get; set; }

    /// <summary>Only one encounter per campaign is ever active at a time. Ending combat sets
    /// this to false (archiving it) rather than deleting, so history is kept.</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}
