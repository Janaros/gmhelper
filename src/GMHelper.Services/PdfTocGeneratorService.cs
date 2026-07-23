using GMHelper.Core.Abstractions;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Interactive;
using Syncfusion.Pdf.Parsing;

namespace GMHelper.Services;

/// <summary>
/// Font-size based heading detection: the most common font size in the document is assumed to be
/// body text; the largest recurring size above that becomes level-1 bookmarks, the next-largest
/// recurring size becomes level-2 bookmarks nested under the most recent level-1 entry. This is
/// independent of language, numbering style or punctuation (regex on heading text like "1. Titel"
/// broke down against real guides that use "1 · Titel", "N.M · Titel", differing separators, or
/// coincidentally-numbered body list items) — it only requires the source PDF to use a
/// consistent font size per heading level, which is true of essentially every generated or
/// typeset guide.
/// </summary>
public class PdfTocGeneratorService : IPdfTocGeneratorService
{
    /// <summary>A candidate heading size must recur on at least this many distinct lines to count
    /// as a real heading tier rather than a one-off decorative element (e.g. a cover-page title).</summary>
    private const int MinimumRecurrenceCount = 2;

    public Task<int> GenerateOutlineAsync(string absoluteFilePath, CancellationToken cancellationToken = default) =>
        Task.Run(() => GenerateOutline(absoluteFilePath), cancellationToken);

    private static int GenerateOutline(string absoluteFilePath)
    {
        using var document = new PdfLoadedDocument(absoluteFilePath);

        var lines = ExtractLines(document);
        var (level1Size, level2Size) = DetermineHeadingSizes(lines);

        if (level1Size is null)
        {
            document.Close(true);
            return 0;
        }

        document.Bookmarks.Clear();

        PdfBookmark? currentTopLevel = null;
        var count = 0;

        foreach (var line in lines)
        {
            if (line.FontSize == level1Size)
            {
                currentTopLevel = document.Bookmarks.Add(line.Text);
                currentTopLevel.Destination = new PdfDestination(document.Pages[line.PageIndex]);
                count++;
            }
            else if (level2Size is not null && line.FontSize == level2Size)
            {
                var bookmark = currentTopLevel is null
                    ? document.Bookmarks.Add(line.Text)
                    : currentTopLevel.Add(line.Text);
                bookmark.Destination = new PdfDestination(document.Pages[line.PageIndex]);
                count++;
            }
        }

        if (count > 0)
        {
            document.Save(absoluteFilePath);
        }

        document.Close(true);
        return count;
    }

    private static List<Line> ExtractLines(PdfLoadedDocument document)
    {
        var lines = new List<Line>();

        for (var pageIndex = 0; pageIndex < document.Pages.Count; pageIndex++)
        {
            var page = (PdfLoadedPage)document.Pages[pageIndex];
            page.ExtractText(out List<TextData> textData);

            foreach (var group in textData.GroupBy(t => MathF.Round(t.Bounds.Y, 0)))
            {
                var text = string.Concat(group.Select(t => t.Text)).Trim();
                if (text.Length == 0 || text.Length > 150)
                {
                    continue;
                }

                var fontSize = MathF.Round(group.Max(t => t.FontSize), 1);
                lines.Add(new Line(pageIndex, text, fontSize));
            }
        }

        return lines;
    }

    /// <summary>Picks the two largest font sizes that (a) exceed the document's most common
    /// (body-text) size and (b) recur often enough to be a real heading tier, not decoration.</summary>
    private static (float? Level1, float? Level2) DetermineHeadingSizes(List<Line> lines)
    {
        var countsBySize = lines
            .GroupBy(l => l.FontSize)
            .ToDictionary(g => g.Key, g => g.Count());

        if (countsBySize.Count == 0)
        {
            return (null, null);
        }

        var bodySize = countsBySize.OrderByDescending(kv => kv.Value).First().Key;

        var headingSizes = countsBySize
            .Where(kv => kv.Key > bodySize && kv.Value >= MinimumRecurrenceCount)
            .OrderByDescending(kv => kv.Key)
            .Select(kv => kv.Key)
            .ToList();

        return headingSizes.Count switch
        {
            0 => (null, null),
            1 => (headingSizes[0], null),
            _ => (headingSizes[0], headingSizes[1]),
        };
    }

    private sealed record Line(int PageIndex, string Text, float FontSize);
}
