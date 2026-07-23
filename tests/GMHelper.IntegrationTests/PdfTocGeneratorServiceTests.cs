using System.Drawing;
using GMHelper.Services;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;

namespace GMHelper.IntegrationTests;

public class PdfTocGeneratorServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly PdfTocGeneratorService _sut = new();

    public PdfTocGeneratorServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "GMHelperTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    [Fact]
    public async Task GenerateOutlineAsync_NumberedAndLetteredHeadings_CreatesNestedBookmarks()
    {
        var path = Path.Combine(_tempRoot, "guide.pdf");
        CreateFixturePdf(path);

        var count = await _sut.GenerateOutlineAsync(path);

        Assert.Equal(4, count);

        using var loaded = new PdfLoadedDocument(path);
        Assert.Equal(2, loaded.Bookmarks.Count);
        Assert.Equal("1. Uebersicht und Ziele", loaded.Bookmarks[0].Title);
        Assert.Equal(0, loaded.Bookmarks[0].Count);

        Assert.Equal("2. Ankunft in Phandalin", loaded.Bookmarks[1].Title);
        Assert.Equal(2, loaded.Bookmarks[1].Count);
        Assert.Equal("A. Trillian Edermath", loaded.Bookmarks[1][0].Title);
        Assert.Equal("B. Sister Garaele", loaded.Bookmarks[1][1].Title);
        loaded.Close(true);
    }

    [Fact]
    public async Task GenerateOutlineAsync_NoHeadings_ReturnsZeroAndLeavesFileUntouched()
    {
        var path = Path.Combine(_tempRoot, "plain.pdf");
        using (var document = new PdfDocument())
        {
            var font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
            document.Pages.Add().Graphics.DrawString("Just a normal paragraph, no headings here.", font, PdfBrushes.Black, new PointF(10, 10));
            document.Save(path);
        }
        var bytesBefore = await File.ReadAllBytesAsync(path);

        var count = await _sut.GenerateOutlineAsync(path);

        Assert.Equal(0, count);
        Assert.Equal(bytesBefore, await File.ReadAllBytesAsync(path));
    }

    /// <summary>Mirrors real guide PDFs: headings use distinctly larger font sizes than body
    /// text (detection is font-size based, not text-pattern based - see PdfTocGeneratorService),
    /// and body text must stay the single most frequent size so it's recognized as the baseline.
    /// Both heading sizes must exceed 14pt specifically: this test project has no Syncfusion
    /// license registered, so Syncfusion.Pdf renders its unlicensed-trial watermark text at a
    /// recurring 14pt across every page, which would otherwise outrank a lower heading tier and
    /// get picked up as one (harmless in the shipped app, which does register a license - see
    /// App.xaml.cs - but very much present in a bare test run).</summary>
    private static void CreateFixturePdf(string path)
    {
        using var document = new PdfDocument();
        var bodyFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
        var level1Font = new PdfStandardFont(PdfFontFamily.Helvetica, 19);
        var level2Font = new PdfStandardFont(PdfFontFamily.Helvetica, 15);

        var page1 = document.Pages.Add();
        page1.Graphics.DrawString("1. Uebersicht und Ziele", level1Font, PdfBrushes.Black, new PointF(10, 10));
        page1.Graphics.DrawString("Kapitel 5 markiert den Uebergang.", bodyFont, PdfBrushes.Black, new PointF(10, 40));
        page1.Graphics.DrawString("Die Helden kehren nach Phandalin zurueck.", bodyFont, PdfBrushes.Black, new PointF(10, 60));
        page1.Graphics.DrawString("Goblins haben die Kontrolle uebernommen.", bodyFont, PdfBrushes.Black, new PointF(10, 80));
        page1.Graphics.DrawString("Die Stadt hat sich veraendert.", bodyFont, PdfBrushes.Black, new PointF(10, 100));

        var page2 = document.Pages.Add();
        page2.Graphics.DrawString("2. Ankunft in Phandalin", level1Font, PdfBrushes.Black, new PointF(10, 10));
        page2.Graphics.DrawString("A. Trillian Edermath", level2Font, PdfBrushes.Black, new PointF(10, 40));
        page2.Graphics.DrawString("B. Sister Garaele", level2Font, PdfBrushes.Black, new PointF(10, 70));
        page2.Graphics.DrawString("Die Ruecckehr fuehlt sich falsch an.", bodyFont, PdfBrushes.Black, new PointF(10, 100));
        page2.Graphics.DrawString("Goblin-Wachen patrouillieren offen.", bodyFont, PdfBrushes.Black, new PointF(10, 120));
        page2.Graphics.DrawString("Buerger senken den Blick.", bodyFont, PdfBrushes.Black, new PointF(10, 140));
        page2.Graphics.DrawString("Fensterlaeden sind geschlossen.", bodyFont, PdfBrushes.Black, new PointF(10, 160));

        document.Save(path);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }
}
