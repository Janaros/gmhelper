using GMHelper.Core.Enums;
using GMHelper.Data;
using GMHelper.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GMHelper.IntegrationTests;

public class MonsterImportExportServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly ServiceProvider _serviceProvider;
    private readonly MonsterService _monsterService;
    private readonly StatFieldService _statFieldService;
    private readonly ImageLibraryService _imageLibraryService;
    private readonly MonsterImportService _importService;
    private readonly MonsterExportService _exportService;

    public MonsterImportExportServiceTests()
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

        _monsterService = new MonsterService(factory, appPaths);
        _statFieldService = new StatFieldService(factory);
        _imageLibraryService = new ImageLibraryService(factory, appPaths);
        _importService = new MonsterImportService(_monsterService, _statFieldService, _imageLibraryService);
        _exportService = new MonsterExportService(_monsterService, _statFieldService, _imageLibraryService);
    }

    [Fact]
    public async Task ParseJsonAsync_ReadsNameNotesImagePathAndStats()
    {
        var jsonPath = Path.Combine(_tempRoot, "import.json");
        await File.WriteAllTextAsync(jsonPath, """
        [
          { "name": "Goblin", "notes": "Schwach", "stats": [ { "name": "AC", "value": "15" }, { "name": "HP", "value": "7" } ] }
        ]
        """);

        var records = await _importService.ParseJsonAsync(jsonPath);

        Assert.Single(records);
        Assert.Equal("Goblin", records[0].Name);
        Assert.Equal("Schwach", records[0].Notes);
        Assert.Equal(["AC", "HP"], records[0].Stats.Select(s => s.Name));
    }

    [Fact]
    public async Task ParseCsvAsync_TreatsNonReservedColumnsAsStats()
    {
        var csvPath = Path.Combine(_tempRoot, "import.csv");
        await File.WriteAllTextAsync(csvPath, "Name,Notes,AC,HP\nGoblin,Schwach,15,7\nOrk,,13,15\n");

        var records = await _importService.ParseCsvAsync(csvPath);

        Assert.Equal(2, records.Count);
        Assert.Equal("Goblin", records[0].Name);
        Assert.Equal(["AC", "HP"], records[0].Stats.Select(s => s.Name));
        Assert.Equal(["15", "7"], records[0].Stats.Select(s => s.Value));
        Assert.Equal(string.Empty, records[1].Notes);
    }

    [Fact]
    public async Task CommitImportAsync_NewMonster_CreatesMonsterAndStatFields()
    {
        var jsonPath = Path.Combine(_tempRoot, "import.json");
        await File.WriteAllTextAsync(jsonPath, """
        [ { "name": "Goblin", "stats": [ { "name": "AC", "value": "15" } ] } ]
        """);
        var records = await _importService.ParseJsonAsync(jsonPath);

        var result = await _importService.CommitImportAsync(records, _tempRoot, MonsterImportConflictStrategy.Skip);

        Assert.Equal(1, result.CreatedCount);
        var monsters = await _monsterService.GetMonstersAsync();
        var monster = Assert.Single(monsters);
        var fields = await _statFieldService.GetStatFieldsAsync(StatFieldOwnerType.Monster, monster.Id);
        Assert.Equal("AC", fields.Single().Name);
    }

    [Theory]
    [InlineData(MonsterImportConflictStrategy.Skip, 1, 0, 1)]
    [InlineData(MonsterImportConflictStrategy.Overwrite, 1, 1, 0)]
    [InlineData(MonsterImportConflictStrategy.CreateDuplicate, 2, 0, 0)]
    public async Task CommitImportAsync_NameConflict_RespectsStrategy(
        MonsterImportConflictStrategy strategy, int expectedTotalMonsters, int expectedUpdated, int expectedSkipped)
    {
        await _monsterService.CreateMonsterAsync("Goblin");

        var jsonPath = Path.Combine(_tempRoot, "import.json");
        await File.WriteAllTextAsync(jsonPath, """
        [ { "name": "Goblin", "notes": "Aktualisiert", "stats": [] } ]
        """);
        var records = await _importService.ParseJsonAsync(jsonPath);

        var result = await _importService.CommitImportAsync(records, _tempRoot, strategy);

        Assert.Equal(expectedUpdated, result.UpdatedCount);
        Assert.Equal(expectedSkipped, result.SkippedCount);
        var monsters = await _monsterService.GetMonstersAsync();
        Assert.Equal(expectedTotalMonsters, monsters.Count);
    }

    [Fact]
    public async Task ExportToJsonAsync_ThenParseJsonAsync_RoundTripsNameNotesAndStats()
    {
        var monster = await _monsterService.CreateMonsterAsync("Goblin");
        await _monsterService.UpdateMonsterAsync(monster.Id, "Goblin", "Schwacher Gegner", null);
        await _statFieldService.ReplaceStatFieldsAsync(StatFieldOwnerType.Monster, monster.Id, [("AC", "15"), ("HP", "7")]);

        var exportPath = Path.Combine(_tempRoot, "export", "monsters.json");
        Directory.CreateDirectory(Path.GetDirectoryName(exportPath)!);
        await _exportService.ExportToJsonAsync(exportPath);

        var reimported = await _importService.ParseJsonAsync(exportPath);

        var record = Assert.Single(reimported);
        Assert.Equal("Goblin", record.Name);
        Assert.Equal("Schwacher Gegner", record.Notes);
        Assert.Equal(["AC", "HP"], record.Stats.Select(s => s.Name));
    }

    [Fact]
    public async Task ExportToJsonAsync_WithImage_CopiesImageAndReferencesRelativePath()
    {
        var monster = await _monsterService.CreateMonsterAsync("Goblin");
        var sourceImagePath = Path.Combine(_tempRoot, "goblin.png");
        await File.WriteAllBytesAsync(sourceImagePath, [0x89, 0x50, 0x4E, 0x47]);
        var image = await _imageLibraryService.AddImageAsync(ImageOwnerType.Monster, monster.Id, sourceImagePath, ImageCategory.Monster);
        await _monsterService.UpdateMonsterAsync(monster.Id, "Goblin", null, image.Id);

        var exportPath = Path.Combine(_tempRoot, "export2", "monsters.json");
        Directory.CreateDirectory(Path.GetDirectoryName(exportPath)!);
        await _exportService.ExportToJsonAsync(exportPath);

        var reimported = await _importService.ParseJsonAsync(exportPath);
        var record = Assert.Single(reimported);
        Assert.NotNull(record.ImagePath);

        var absoluteImagePath = Path.Combine(Path.GetDirectoryName(exportPath)!, record.ImagePath!);
        Assert.True(File.Exists(absoluteImagePath));
    }

    [Fact]
    public async Task ExportToCsvAsync_WritesHeaderWithUnionOfStatColumns()
    {
        var goblin = await _monsterService.CreateMonsterAsync("Goblin");
        await _statFieldService.ReplaceStatFieldsAsync(StatFieldOwnerType.Monster, goblin.Id, [("AC", "15")]);
        var ork = await _monsterService.CreateMonsterAsync("Ork");
        await _statFieldService.ReplaceStatFieldsAsync(StatFieldOwnerType.Monster, ork.Id, [("HP", "20")]);

        var exportPath = Path.Combine(_tempRoot, "export.csv");
        await _exportService.ExportToCsvAsync(exportPath);

        var lines = await File.ReadAllLinesAsync(exportPath);
        Assert.Equal("Name,Notes,ImagePath,AC,HP", lines[0]);
        Assert.Equal(3, lines.Length);
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
