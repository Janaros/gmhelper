using GMHelper.Data;
using GMHelper.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GMHelper.IntegrationTests;

public class SessionNotesServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly ServiceProvider _serviceProvider;
    private readonly SessionNotesService _sut;
    private readonly int _campaignId;

    public SessionNotesServiceTests()
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
            _campaignId = campaign.Id;
        }

        _sut = new SessionNotesService(factory);
    }

    [Fact]
    public async Task CreateNoteAsync_PersistsRow()
    {
        var note = await _sut.CreateNoteAsync(_campaignId, "Session 1", new DateTime(2026, 7, 1), "# Die Goblin-Höhle");

        var notes = await _sut.GetNotesForCampaignAsync(_campaignId);
        var found = notes.Single(n => n.Id == note.Id);
        Assert.Equal("Session 1", found.Title);
        Assert.Equal("# Die Goblin-Höhle", found.MarkdownContent);
    }

    [Fact]
    public async Task GetNotesForCampaignAsync_OrdersByMostRecentSessionDateFirst()
    {
        await _sut.CreateNoteAsync(_campaignId, "Ältere Session", new DateTime(2026, 6, 1), "");
        await _sut.CreateNoteAsync(_campaignId, "Neuere Session", new DateTime(2026, 7, 1), "");

        var notes = await _sut.GetNotesForCampaignAsync(_campaignId);

        Assert.Equal(["Neuere Session", "Ältere Session"], notes.Select(n => n.Title));
    }

    [Fact]
    public async Task UpdateNoteAsync_UpdatesContentAndTimestamp()
    {
        var note = await _sut.CreateNoteAsync(_campaignId, "Session 1", DateTime.Today, "Alter Text");

        await _sut.UpdateNoteAsync(note.Id, "Session 1 (bearbeitet)", DateTime.Today, "Neuer Text");

        var notes = await _sut.GetNotesForCampaignAsync(_campaignId);
        var updated = notes.Single(n => n.Id == note.Id);
        Assert.Equal("Session 1 (bearbeitet)", updated.Title);
        Assert.Equal("Neuer Text", updated.MarkdownContent);
        Assert.True(updated.UpdatedAt >= updated.CreatedAt);
    }

    [Fact]
    public async Task DeleteNoteAsync_RemovesRow()
    {
        var note = await _sut.CreateNoteAsync(_campaignId, "Session 1", DateTime.Today, "");

        await _sut.DeleteNoteAsync(note.Id);

        var notes = await _sut.GetNotesForCampaignAsync(_campaignId);
        Assert.DoesNotContain(notes, n => n.Id == note.Id);
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
