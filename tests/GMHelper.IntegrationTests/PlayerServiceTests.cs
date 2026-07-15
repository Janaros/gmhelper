using GMHelper.Data;
using GMHelper.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GMHelper.IntegrationTests;

public class PlayerServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly ServiceProvider _serviceProvider;
    private readonly PlayerService _sut;
    private readonly int _campaignId;

    public PlayerServiceTests()
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

        _sut = new PlayerService(factory);
    }

    [Fact]
    public async Task CreatePlayerAsync_PersistsRow()
    {
        var player = await _sut.CreatePlayerAsync(_campaignId, "Grog");

        var players = await _sut.GetPlayersForCampaignAsync(_campaignId);
        Assert.Contains(players, p => p.Id == player.Id && p.CharacterName == "Grog");
    }

    [Fact]
    public async Task UpdatePlayerAsync_UpdatesInitiativeAndOtherFields()
    {
        var player = await _sut.CreatePlayerAsync(_campaignId, "Grog");

        await _sut.UpdatePlayerAsync(player.Id, "Grog Strongjaw", "Travis", 18, "Barbar");

        var players = await _sut.GetPlayersForCampaignAsync(_campaignId);
        var updated = players.Single(p => p.Id == player.Id);
        Assert.Equal("Grog Strongjaw", updated.CharacterName);
        Assert.Equal("Travis", updated.PlayerName);
        Assert.Equal(18, updated.Initiative);
        Assert.Equal("Barbar", updated.Notes);
    }

    [Fact]
    public async Task DeletePlayerAsync_RemovesRow()
    {
        var player = await _sut.CreatePlayerAsync(_campaignId, "Grog");

        await _sut.DeletePlayerAsync(player.Id);

        var players = await _sut.GetPlayersForCampaignAsync(_campaignId);
        Assert.DoesNotContain(players, p => p.Id == player.Id);
    }

    [Fact]
    public async Task SetActiveAsync_TogglesFlag_ButKeepsPlayerInRoster()
    {
        var player = await _sut.CreatePlayerAsync(_campaignId, "Grog");

        await _sut.SetActiveAsync(player.Id, false);

        var players = await _sut.GetPlayersForCampaignAsync(_campaignId);
        var found = players.Single(p => p.Id == player.Id);
        Assert.False(found.IsActive);
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
