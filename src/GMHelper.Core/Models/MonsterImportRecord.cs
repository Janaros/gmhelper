namespace GMHelper.Core.Models;

/// <summary>
/// One monster as read from an import file (JSON or CSV) — also the shape written back out
/// on export, so a round-trip export→import is lossless.
/// </summary>
public class MonsterImportRecord
{
    public required string Name { get; set; }
    public string? Notes { get; set; }

    /// <summary>Path to an image file, resolved relative to the import/export file's own directory.</summary>
    public string? ImagePath { get; set; }

    public List<MonsterImportStatField> Stats { get; set; } = new();
}
