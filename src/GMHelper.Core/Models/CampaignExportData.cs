using GMHelper.Core.Entities;

namespace GMHelper.Core.Models;

/// <summary>
/// Flat snapshot of everything belonging to a campaign, serialized to JSON as part of the
/// backup export. Not meant as a stable/versioned re-import schema — this is a GM safety-net
/// backup, not a portable exchange format (see IMonsterImportService/Record for that use case).
/// </summary>
public class CampaignExportData
{
    public required Campaign Campaign { get; set; }
    public List<PdfDocument> PdfDocuments { get; set; } = new();
    public List<ImageAsset> ImageAssets { get; set; } = new();
    public List<Player> Players { get; set; } = new();
    public List<StatField> PlayerStatFields { get; set; } = new();
    public List<SessionNote> SessionNotes { get; set; } = new();
    public List<CombatEncounter> CombatEncounters { get; set; } = new();
    public List<CombatParticipant> CombatParticipants { get; set; } = new();
}
