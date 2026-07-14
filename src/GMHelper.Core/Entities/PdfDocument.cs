namespace GMHelper.Core.Entities;

public class PdfDocument
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public required string FileName { get; set; }
    public required string StoredRelativePath { get; set; }
    public int DisplayOrder { get; set; }
    public int LastViewedPage { get; set; } = 1;
    public DateTime AddedAt { get; set; }
}
