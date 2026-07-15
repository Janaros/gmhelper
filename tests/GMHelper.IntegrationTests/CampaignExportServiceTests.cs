using System.IO.Compression;
using System.Text.Json;
using GMHelper.Core.Enums;
using GMHelper.Core.Models;
using GMHelper.Data;
using GMHelper.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GMHelper.IntegrationTests;

public class CampaignExportServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly ServiceProvider _serviceProvider;
    private readonly CampaignExportService _sut;
    private readonly int _campaignId;
    private readonly AppPaths _appPaths;

    public CampaignExportServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "GMHelperTests", Guid.NewGuid().ToString("N"));
        _appPaths = new AppPaths(_tempRoot);

        var services = new ServiceCollection();
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={_appPaths.DatabaseFilePath}"));
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

            var player = new Core.Entities.Player { CampaignId = campaign.Id, CharacterName = "Grog", Initiative = 15 };
            db.Players.Add(player);
            db.SaveChanges();

            db.StatFields.Add(new Core.Entities.StatField { OwnerType = StatFieldOwnerType.Player, OwnerId = player.Id, Name = "HP", Value = "45", SortOrder = 0 });

            db.SessionNotes.Add(new Core.Entities.SessionNote { CampaignId = campaign.Id, Title = "Session 1", SessionDate = DateTime.Today, MarkdownContent = "# Hallo" });

            db.SaveChanges();
        }

        var pdfsFolder = _appPaths.CampaignPdfsFolder(_campaignId);
        Directory.CreateDirectory(pdfsFolder);
        File.WriteAllText(Path.Combine(pdfsFolder, "abenteuer.pdf"), "%PDF-1.4 dummy");

        _sut = new CampaignExportService(factory, _appPaths);
    }

    [Fact]
    public async Task ExportCampaignAsync_WritesJsonWithCampaignDataAndPlayerStatFields()
    {
        var zipPath = Path.Combine(_tempRoot, "export.zip");

        await _sut.ExportCampaignAsync(_campaignId, zipPath);

        using var archive = ZipFile.OpenRead(zipPath);
        var jsonEntry = archive.GetEntry("campaign-data.json");
        Assert.NotNull(jsonEntry);

        using var reader = new StreamReader(jsonEntry!.Open());
        var json = await reader.ReadToEndAsync();
        var data = JsonSerializer.Deserialize<CampaignExportData>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(data);
        Assert.Equal("Testkampagne", data!.Campaign.Name);
        Assert.Single(data.Players);
        Assert.Single(data.PlayerStatFields);
        Assert.Equal("HP", data.PlayerStatFields[0].Name);
        Assert.Single(data.SessionNotes);
    }

    [Fact]
    public async Task ExportCampaignAsync_IncludesCampaignFilesInZip()
    {
        var zipPath = Path.Combine(_tempRoot, "export2.zip");

        await _sut.ExportCampaignAsync(_campaignId, zipPath);

        using var archive = ZipFile.OpenRead(zipPath);
        Assert.Contains(archive.Entries, e => e.FullName.EndsWith("abenteuer.pdf", StringComparison.OrdinalIgnoreCase));
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
