using GMHelper.Core.Enums;
using GMHelper.Data;
using GMHelper.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GMHelper.IntegrationTests;

public class StatFieldServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly ServiceProvider _serviceProvider;
    private readonly StatFieldService _sut;

    public StatFieldServiceTests()
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

        _sut = new StatFieldService(factory);
    }

    [Fact]
    public async Task ReplaceStatFieldsAsync_PersistsFieldsInOrder()
    {
        await _sut.ReplaceStatFieldsAsync(StatFieldOwnerType.Player, 1, [("AC", "15"), ("HP", "22")]);

        var fields = await _sut.GetStatFieldsAsync(StatFieldOwnerType.Player, 1);

        Assert.Equal(["AC", "HP"], fields.Select(f => f.Name));
        Assert.Equal(["15", "22"], fields.Select(f => f.Value));
        Assert.Equal([0, 1], fields.Select(f => f.SortOrder));
    }

    [Fact]
    public async Task ReplaceStatFieldsAsync_CalledTwice_ReplacesPreviousSet()
    {
        await _sut.ReplaceStatFieldsAsync(StatFieldOwnerType.Player, 1, [("AC", "15")]);
        await _sut.ReplaceStatFieldsAsync(StatFieldOwnerType.Player, 1, [("Willenskraft", "12")]);

        var fields = await _sut.GetStatFieldsAsync(StatFieldOwnerType.Player, 1);

        Assert.Single(fields);
        Assert.Equal("Willenskraft", fields[0].Name);
    }

    [Fact]
    public async Task GetStatFieldsAsync_DoesNotMixDifferentOwnerTypes()
    {
        await _sut.ReplaceStatFieldsAsync(StatFieldOwnerType.Player, 1, [("AC", "15")]);
        await _sut.ReplaceStatFieldsAsync(StatFieldOwnerType.Monster, 1, [("CR", "3")]);

        var playerFields = await _sut.GetStatFieldsAsync(StatFieldOwnerType.Player, 1);

        Assert.Single(playerFields);
        Assert.Equal("AC", playerFields[0].Name);
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
