using GMHelper.Core.Enums;

namespace GMHelper.Core.Entities;

public class ImageAsset
{
    public int Id { get; set; }
    public ImageOwnerType OwnerType { get; set; }
    public int OwnerId { get; set; }
    public required string FileName { get; set; }
    public required string StoredRelativePath { get; set; }
    public ImageCategory Category { get; set; }
    public DateTime AddedAt { get; set; }
}
