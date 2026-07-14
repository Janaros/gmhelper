using GMHelper.Data;
using GMHelper.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GMHelper.IntegrationTests;

public class MonsterServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly ServiceProvider _serviceProvider;
    private readonly MonsterService _sut;

    public MonsterServiceTests()
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
        }

        _sut = new MonsterService(factory, appPaths);
    }

    [Fact]
    public async Task CreateMonsterAsync_PersistsRowWithManualSource()
    {
        var monster = await _sut.CreateMonsterAsync("Goblin");

        var monsters = await _sut.GetMonstersAsync();
        var found = monsters.Single(m => m.Id == monster.Id);
        Assert.Equal("Goblin", found.Name);
        Assert.Equal("Manual", found.Source);
    }

    [Fact]
    public async Task UpdateMonsterAsync_UpdatesNameNotesAndImageAssetId()
    {
        var monster = await _sut.CreateMonsterAsync("Goblin");

        await _sut.UpdateMonsterAsync(monster.Id, "Goblin Boss", "Anführer der Bande", 42);

        var monsters = await _sut.GetMonstersAsync();
        var updated = monsters.Single(m => m.Id == monster.Id);
        Assert.Equal("Goblin Boss", updated.Name);
        Assert.Equal("Anführer der Bande", updated.Notes);
        Assert.Equal(42, updated.ImageAssetId);
    }

    [Fact]
    public async Task DeleteMonsterAsync_RemovesRowAndFolder()
    {
        var monster = await _sut.CreateMonsterAsync("Goblin");
        var appPaths = new AppPaths(_tempRoot);
        var folder = appPaths.MonsterFolder(monster.Id);
        Directory.CreateDirectory(folder);

        await _sut.DeleteMonsterAsync(monster.Id);

        var monsters = await _sut.GetMonstersAsync();
        Assert.DoesNotContain(monsters, m => m.Id == monster.Id);
        Assert.False(Directory.Exists(folder));
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
