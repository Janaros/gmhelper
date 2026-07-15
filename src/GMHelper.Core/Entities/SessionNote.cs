namespace GMHelper.Core.Entities;

public class SessionNote
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public required string Title { get; set; }
    public DateTime SessionDate { get; set; }
    public string MarkdownContent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
