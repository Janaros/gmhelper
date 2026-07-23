namespace GMHelper.Core.Entities;

/// <summary>GM-curated named jump point into a PDF (e.g. "Goblin-Hinterhalt"), independent of
/// whatever bookmarks the PDF itself may or may not contain.</summary>
public class PdfJumpMarker
{
    public int Id { get; set; }
    public int PdfDocumentId { get; set; }
    public required string Title { get; set; }
    public int PageNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}
