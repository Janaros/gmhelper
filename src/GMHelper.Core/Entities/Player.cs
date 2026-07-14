namespace GMHelper.Core.Entities;

public class Player
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public required string CharacterName { get; set; }
    public string? PlayerName { get; set; }
    public int? Initiative { get; set; }
    public int? PortraitImageAssetId { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}
