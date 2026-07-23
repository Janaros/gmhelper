using GMHelper.Data;
using GMHelper.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GMHelper.IntegrationTests;

public class PdfJumpMarkerServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly ServiceProvider _serviceProvider;
    private readonly PdfJumpMarkerService _sut;
    private readonly int _pdfDocumentId;

    public PdfJumpMarkerServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "GMHelperTests", Guid.NewGuid().ToString("N"));
        var appPaths = new AppPaths(_tempRoot);

        var services = new ServiceCollection();
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={appPaths.DatabaseFilePath}"));
        _serviceProvider = services.BuildServiceProvider();

        var factory = _serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using (var db = factory.CreateDbContext())
        {
            Directory.CreateDirectory(_tempRoot);
            db.Database.Migrate();

            var campaign = new Core.Entities.Campaign { Name = "Testkampagne", CreatedAt = DateTime.UtcNow };
            db.Campaigns.Add(campaign);
            db.SaveChanges();

            var pdfDocument = new Core.Entities.PdfDocument
            {
                CampaignId = campaign.Id,
                FileName = "guide.pdf",
                StoredRelativePath = "guide.pdf",
                AddedAt = DateTime.UtcNow,
            };
            db.PdfDocuments.Add(pdfDocument);
            db.SaveChanges();
            _pdfDocumentId = pdfDocument.Id;
        }

        _sut = new PdfJumpMarkerService(factory);
    }

    [Fact]
    public async Task AddJumpMarkerAsync_PersistsMarker()
    {
        var marker = await _sut.AddJumpMarkerAsync(_pdfDocumentId, "Goblin-Hinterhalt", 7);

        var markers = await _sut.GetJumpMarkersAsync(_pdfDocumentId);
        Assert.Single(markers);
        Assert.Equal("Goblin-Hinterhalt", markers[0].Title);
        Assert.Equal(7, markers[0].PageNumber);
        Assert.Equal(marker.Id, markers[0].Id);
    }

    [Fact]
    public async Task GetJumpMarkersAsync_OrdersByPageNumber()
    {
        await _sut.AddJumpMarkerAsync(_pdfDocumentId, "Später", 12);
        await _sut.AddJumpMarkerAsync(_pdfDocumentId, "Früher", 3);

        var markers = await _sut.GetJumpMarkersAsync(_pdfDocumentId);

        Assert.Equal(["Früher", "Später"], markers.Select(m => m.Title));
    }

    [Fact]
    public async Task DeleteJumpMarkerAsync_RemovesMarker()
    {
        var marker = await _sut.AddJumpMarkerAsync(_pdfDocumentId, "Goblin-Hinterhalt", 7);

        await _sut.DeleteJumpMarkerAsync(marker.Id);

        var markers = await _sut.GetJumpMarkersAsync(_pdfDocumentId);
        Assert.Empty(markers);
    }

    [Fact]
    public async Task DeleteJumpMarkerAsync_UnknownId_DoesNotThrow()
    {
        await _sut.DeleteJumpMarkerAsync(999);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        SqliteConnection.ClearAllPools();

        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }
}
