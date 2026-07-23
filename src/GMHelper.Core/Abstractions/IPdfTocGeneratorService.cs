namespace GMHelper.Core.Abstractions;

/// <summary>
/// Scans a PDF's text for numbered headings and writes them as a real PDF outline (visible in
/// the viewer's native bookmark panel) — most GM guide PDFs have a clear heading structure but
/// no embedded bookmarks at all.
/// </summary>
public interface IPdfTocGeneratorService
{
    /// <summary>Returns the number of bookmark entries created (both nesting levels combined).
    /// If no headings are detected, the file is left untouched and 0 is returned.</summary>
    Task<int> GenerateOutlineAsync(string absoluteFilePath, CancellationToken cancellationToken = default);
}
