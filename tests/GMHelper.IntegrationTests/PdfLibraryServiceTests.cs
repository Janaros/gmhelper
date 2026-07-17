using GMHelper.Data;
using GMHelper.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GMHelper.IntegrationTests;

public class PdfLibraryServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly string _sourcePdfPath;
    private readonly ServiceProvider _serviceProvider;
    private readonly PdfLibraryService _sut;
    private readonly int _campaignId;

    public PdfLibraryServiceTests()
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

        _sourcePdfPath = Path.Combine(_tempRoot, "source.pdf");
        File.WriteAllText(_sourcePdfPath, "%PDF-1.4 dummy content for tests");

        _sut = new PdfLibraryService(factory, appPaths);
    }

    [Fact]
    public async Task AddPdfToCampaignAsync_CopiesFileAndPersistsRow()
    {
        var pdf = await _sut.AddPdfToCampaignAsync(_campaignId, _sourcePdfPath);

        Assert.Equal("source.pdf", pdf.FileName);
        Assert.True(File.Exists(_sut.GetAbsoluteFilePath(pdf)));

        var pdfs = await _sut.GetPdfsForCampaignAsync(_campaignId);
        Assert.Contains(pdfs, p => p.Id == pdf.Id);
    }

    [Fact]
    public async Task AddPdfToCampaignAsync_TwiceWithSameFileName_CreatesDistinctFiles()
    {
        var first = await _sut.AddPdfToCampaignAsync(_campaignId, _sourcePdfPath);
        var second = await _sut.AddPdfToCampaignAsync(_campaignId, _sourcePdfPath);

        Assert.NotEqual(_sut.GetAbsoluteFilePath(first), _sut.GetAbsoluteFilePath(second));
        Assert.True(File.Exists(_sut.GetAbsoluteFilePath(first)));
        Assert.True(File.Exists(_sut.GetAbsoluteFilePath(second)));
    }

    [Fact]
    public async Task UpdateLastViewedPageAsync_PersistsPageNumber()
    {
        var pdf = await _sut.AddPdfToCampaignAsync(_campaignId, _sourcePdfPath);

        await _sut.UpdateLastViewedPageAsync(pdf.Id, 7);

        var pdfs = await _sut.GetPdfsForCampaignAsync(_campaignId);
        Assert.Equal(7, pdfs.Single(p => p.Id == pdf.Id).LastViewedPage);
    }

    [Fact]
    public async Task CreateBackupAsync_CreatesBakFileWithCurrentContent()
    {
        var pdf = await _sut.AddPdfToCampaignAsync(_campaignId, _sourcePdfPath);

        await _sut.CreateBackupAsync(pdf);

        var backupPath = _sut.GetAbsoluteFilePath(pdf) + ".bak";
        Assert.True(File.Exists(backupPath));
        Assert.Equal(File.ReadAllText(_sut.GetAbsoluteFilePath(pdf)), File.ReadAllText(backupPath));
    }

    [Fact]
    public async Task DeletePdfAsync_RemovesFileAndRow()
    {
        var pdf = await _sut.AddPdfToCampaignAsync(_campaignId, _sourcePdfPath);
        var filePath = _sut.GetAbsoluteFilePath(pdf);

        await _sut.DeletePdfAsync(pdf.Id);

        Assert.False(File.Exists(filePath));
        var pdfs = await _sut.GetPdfsForCampaignAsync(_campaignId);
        Assert.DoesNotContain(pdfs, p => p.Id == pdf.Id);
    }

    [Fact]
    public async Task DeletePdfAsync_AlsoRemovesBackupFile()
    {
        var pdf = await _sut.AddPdfToCampaignAsync(_campaignId, _sourcePdfPath);
        await _sut.CreateBackupAsync(pdf);
        var backupPath = _sut.GetAbsoluteFilePath(pdf) + ".bak";

        await _sut.DeletePdfAsync(pdf.Id);

        Assert.False(File.Exists(backupPath));
    }

    [Fact]
    public async Task DeletePdfAsync_UnknownId_DoesNotThrow()
    {
        await _sut.DeletePdfAsync(999);
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
