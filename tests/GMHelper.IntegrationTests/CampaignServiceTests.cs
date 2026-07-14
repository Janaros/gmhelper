using GMHelper.Data;
using GMHelper.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GMHelper.IntegrationTests;

public class CampaignServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly ServiceProvider _serviceProvider;
    private readonly CampaignService _sut;

    public CampaignServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "GMHelperTests", Guid.NewGuid().ToString("N"));
        var appPaths = new AppPaths(_tempRoot);

        var services = new ServiceCollection();
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={appPaths.DatabaseFilePath}"));
        _serviceProvider = services.BuildServiceProvider();

        using (var scope = _serviceProvider.CreateScope())
        {
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var db = factory.CreateDbContext();
            Directory.CreateDirectory(_tempRoot);
            db.Database.Migrate();
        }

        _sut = new CampaignService(
            _serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>(),
            appPaths);
    }

    [Fact]
    public async Task CreateCampaignAsync_PersistsRowAndCreatesFolderTree()
    {
        var campaign = await _sut.CreateCampaignAsync("Verlorene Minen von Phandelver", "Testkampagne");

        var campaigns = await _sut.GetCampaignsAsync();
        Assert.Contains(campaigns, c => c.Id == campaign.Id && c.Name == "Verlorene Minen von Phandelver");

        var appPaths = new AppPaths(_tempRoot);
        Assert.True(Directory.Exists(appPaths.CampaignPdfsFolder(campaign.Id)));
        Assert.True(Directory.Exists(appPaths.CampaignImagesFolder(campaign.Id)));
    }

    [Fact]
    public async Task DeleteCampaignAsync_RemovesRowAndFolderTree()
    {
        var campaign = await _sut.CreateCampaignAsync("Zu löschende Kampagne", description: null);
        var appPaths = new AppPaths(_tempRoot);
        var campaignFolder = appPaths.CampaignFolder(campaign.Id);
        Assert.True(Directory.Exists(campaignFolder));

        await _sut.DeleteCampaignAsync(campaign.Id);

        var campaigns = await _sut.GetCampaignsAsync();
        Assert.DoesNotContain(campaigns, c => c.Id == campaign.Id);
        Assert.False(Directory.Exists(campaignFolder));
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
