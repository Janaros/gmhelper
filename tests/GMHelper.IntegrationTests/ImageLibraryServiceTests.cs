using GMHelper.Core.Enums;
using GMHelper.Data;
using GMHelper.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GMHelper.IntegrationTests;

public class ImageLibraryServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly string _sourceImagePath;
    private readonly ServiceProvider _serviceProvider;
    private readonly ImageLibraryService _sut;
    private readonly int _campaignId;

    public ImageLibraryServiceTests()
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

        _sourceImagePath = Path.Combine(_tempRoot, "source.png");
        File.WriteAllBytes(_sourceImagePath, [0x89, 0x50, 0x4E, 0x47]);

        _sut = new ImageLibraryService(factory, appPaths);
    }

    [Fact]
    public async Task AddImageAsync_CopiesFileAndPersistsRow()
    {
        var image = await _sut.AddImageAsync(ImageOwnerType.Campaign, _campaignId, _sourceImagePath, ImageCategory.Map);

        Assert.Equal("source.png", image.FileName);
        Assert.True(File.Exists(_sut.GetAbsoluteFilePath(image)));

        var images = await _sut.GetImagesAsync(ImageOwnerType.Campaign, _campaignId);
        Assert.Contains(images, i => i.Id == image.Id && i.Category == ImageCategory.Map);
    }

    [Fact]
    public async Task AddImageAsync_TwiceWithSameFileName_CreatesDistinctFiles()
    {
        var first = await _sut.AddImageAsync(ImageOwnerType.Campaign, _campaignId, _sourceImagePath, ImageCategory.Other);
        var second = await _sut.AddImageAsync(ImageOwnerType.Campaign, _campaignId, _sourceImagePath, ImageCategory.Other);

        Assert.NotEqual(_sut.GetAbsoluteFilePath(first), _sut.GetAbsoluteFilePath(second));
    }

    [Fact]
    public async Task GetImagesAsync_DoesNotReturnImagesFromOtherOwners()
    {
        await _sut.AddImageAsync(ImageOwnerType.Campaign, _campaignId, _sourceImagePath, ImageCategory.Other);
        await _sut.AddImageAsync(ImageOwnerType.Monster, 999, _sourceImagePath, ImageCategory.Monster);

        var images = await _sut.GetImagesAsync(ImageOwnerType.Campaign, _campaignId);

        Assert.Single(images);
    }

    [Fact]
    public async Task DeleteImageAsync_RemovesFileAndRow()
    {
        var image = await _sut.AddImageAsync(ImageOwnerType.Campaign, _campaignId, _sourceImagePath, ImageCategory.Map);
        var filePath = _sut.GetAbsoluteFilePath(image);

        await _sut.DeleteImageAsync(image.Id);

        Assert.False(File.Exists(filePath));
        var images = await _sut.GetImagesAsync(ImageOwnerType.Campaign, _campaignId);
        Assert.DoesNotContain(images, i => i.Id == image.Id);
    }

    [Fact]
    public async Task DeleteImageAsync_UnknownId_DoesNotThrow()
    {
        await _sut.DeleteImageAsync(999);
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
