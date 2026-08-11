using GMHelper.Core.Enums;
using GMHelper.Data;
using GMHelper.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GMHelper.IntegrationTests;

public class HerbalismServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly ServiceProvider _serviceProvider;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly HerbalismRegionService _regionService;
    private readonly HerbalismIngredientService _ingredientService;
    private readonly HerbalismSeeder _seeder;

    public HerbalismServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "GMHelperTests", Guid.NewGuid().ToString("N"));
        var appPaths = new AppPaths(_tempRoot);

        var services = new ServiceCollection();
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={appPaths.DatabaseFilePath}"));
        _serviceProvider = services.BuildServiceProvider();

        _dbContextFactory = _serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using (var db = _dbContextFactory.CreateDbContext())
        {
            Directory.CreateDirectory(_tempRoot);
            db.Database.Migrate();
        }

        _regionService = new HerbalismRegionService(_dbContextFactory);
        _ingredientService = new HerbalismIngredientService(_dbContextFactory);
        _seeder = new HerbalismSeeder(_dbContextFactory);
    }

    [Fact]
    public async Task CreateRegionAsync_PersistsRowWithManualSourceAndDefaultDc()
    {
        var region = await _regionService.CreateRegionAsync("Neverwinterwald");

        var regions = await _regionService.GetRegionsAsync();
        var found = regions.Single(r => r.Id == region.Id);
        Assert.Equal("Neverwinterwald", found.Name);
        Assert.Equal("Manual", found.Source);
        Assert.Equal(15, found.DifficultyClass);
    }

    [Fact]
    public async Task UpdateRegionAsync_UpdatesNameTerrainDescriptionAndDc()
    {
        var region = await _regionService.CreateRegionAsync("Gebiet");

        await _regionService.UpdateRegionAsync(region.Id, "Hochmoor", "Moor und Hochland", "Karg und windig.", 20);

        var regions = await _regionService.GetRegionsAsync();
        var updated = regions.Single(r => r.Id == region.Id);
        Assert.Equal("Hochmoor", updated.Name);
        Assert.Equal("Moor und Hochland", updated.Terrain);
        Assert.Equal("Karg und windig.", updated.Description);
        Assert.Equal(20, updated.DifficultyClass);
    }

    [Fact]
    public async Task CreateAndUpdateIngredientAsync_RoundTripsKindRarityAndValue()
    {
        var region = await _regionService.CreateRegionAsync("Neverwinterwald");
        var ingredient = await _ingredientService.CreateIngredientAsync(region.Id, "Mondtau");

        await _ingredientService.UpdateIngredientAsync(
            ingredient.Id,
            "Mondtau",
            IngredientKind.Both,
            IngredientRarity.Rare,
            "Grundstoff hochwertiger Heiltränke.",
            "Nur bei Vollmond.",
            120);

        var ingredients = await _ingredientService.GetIngredientsAsync(region.Id);
        var updated = ingredients.Single(i => i.Id == ingredient.Id);
        Assert.Equal(IngredientKind.Both, updated.Kind);
        Assert.Equal(IngredientRarity.Rare, updated.Rarity);
        Assert.Equal("Grundstoff hochwertiger Heiltränke.", updated.Effect);
        Assert.Equal("Nur bei Vollmond.", updated.Notes);
        Assert.Equal(120, updated.ValueInGoldPieces);
    }

    [Fact]
    public async Task GetIngredientsAsync_ReturnsOnlyTheRequestedRegion()
    {
        var forest = await _regionService.CreateRegionAsync("Neverwinterwald");
        var moor = await _regionService.CreateRegionAsync("Hochmoor");
        await _ingredientService.CreateIngredientAsync(forest.Id, "Glutkappe");
        await _ingredientService.CreateIngredientAsync(moor.Id, "Moorbart");

        var forestIngredients = await _ingredientService.GetIngredientsAsync(forest.Id);

        Assert.Equal("Glutkappe", Assert.Single(forestIngredients).Name);
    }

    [Fact]
    public async Task DeleteRegionAsync_AlsoRemovesItsIngredients()
    {
        var region = await _regionService.CreateRegionAsync("Neverwinterwald");
        await _ingredientService.CreateIngredientAsync(region.Id, "Glutkappe");
        await _ingredientService.CreateIngredientAsync(region.Id, "Silberflechte");

        await _regionService.DeleteRegionAsync(region.Id);

        var regions = await _regionService.GetRegionsAsync();
        Assert.DoesNotContain(regions, r => r.Id == region.Id);

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        Assert.False(await db.HerbalismIngredients.AnyAsync(i => i.HerbalismRegionId == region.Id));
    }

    [Fact]
    public async Task DeleteIngredientAsync_RemovesOnlyThatRow()
    {
        var region = await _regionService.CreateRegionAsync("Neverwinterwald");
        var doomed = await _ingredientService.CreateIngredientAsync(region.Id, "Glutkappe");
        await _ingredientService.CreateIngredientAsync(region.Id, "Silberflechte");

        await _ingredientService.DeleteIngredientAsync(doomed.Id);

        var ingredients = await _ingredientService.GetIngredientsAsync(region.Id);
        Assert.Equal("Silberflechte", Assert.Single(ingredients).Name);
    }

    [Fact]
    public async Task EnsureSeededAsync_CreatesEverySwordCoastRegionWithItsIngredients()
    {
        await _seeder.EnsureSeededAsync();

        var regions = await _regionService.GetRegionsAsync();
        Assert.Equal(HerbalismSeedData.Regions.Count, regions.Count);
        Assert.All(regions, region => Assert.Equal(HerbalismSeeder.SeedSource, region.Source));

        foreach (var seedRegion in HerbalismSeedData.Regions)
        {
            var region = regions.Single(r => r.Name == seedRegion.Name);
            Assert.Equal(seedRegion.DifficultyClass, region.DifficultyClass);

            var ingredients = await _ingredientService.GetIngredientsAsync(region.Id);
            Assert.Equal(seedRegion.Ingredients.Count, ingredients.Count);
        }
    }

    [Fact]
    public async Task EnsureSeededAsync_RunAgain_DoesNotDuplicateOrOverwriteUserEdits()
    {
        await _seeder.EnsureSeededAsync();
        var seeded = (await _regionService.GetRegionsAsync()).First();
        await _regionService.UpdateRegionAsync(seeded.Id, "Eigener Name", "Eigenes Gelände", "Eigene Notiz", 12);

        await _seeder.EnsureSeededAsync();

        var regions = await _regionService.GetRegionsAsync();
        Assert.Equal(HerbalismSeedData.Regions.Count, regions.Count);
        var edited = regions.Single(r => r.Id == seeded.Id);
        Assert.Equal("Eigener Name", edited.Name);
        Assert.Equal(12, edited.DifficultyClass);
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
