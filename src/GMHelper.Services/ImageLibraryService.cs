using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Core.Enums;
using GMHelper.Data;
using Microsoft.EntityFrameworkCore;

namespace GMHelper.Services;

public class ImageLibraryService : IImageLibraryService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IAppPaths _appPaths;

    public ImageLibraryService(IDbContextFactory<AppDbContext> dbContextFactory, IAppPaths appPaths)
    {
        _dbContextFactory = dbContextFactory;
        _appPaths = appPaths;
    }

    public async Task<ImageAsset> AddImageAsync(ImageOwnerType ownerType, int ownerId, string sourceFilePath, ImageCategory category, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var folder = ResolveOwnerFolder(ownerType, ownerId);
        Directory.CreateDirectory(folder);

        var destinationPath = FileNaming.ResolveUniqueDestinationPath(folder, Path.GetFileName(sourceFilePath));
        File.Copy(sourceFilePath, destinationPath);

        var imageAsset = new ImageAsset
        {
            OwnerType = ownerType,
            OwnerId = ownerId,
            FileName = Path.GetFileName(destinationPath),
            StoredRelativePath = Path.GetRelativePath(_appPaths.DataRoot, destinationPath),
            Category = category,
            AddedAt = DateTime.UtcNow,
        };

        db.ImageAssets.Add(imageAsset);
        await db.SaveChangesAsync(cancellationToken);

        return imageAsset;
    }

    public async Task<IReadOnlyList<ImageAsset>> GetImagesAsync(ImageOwnerType ownerType, int ownerId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.ImageAssets
            .AsNoTracking()
            .Where(i => i.OwnerType == ownerType && i.OwnerId == ownerId)
            .OrderBy(i => i.AddedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteImageAsync(int imageAssetId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var imageAsset = await db.ImageAssets.FindAsync([imageAssetId], cancellationToken);
        if (imageAsset is null)
        {
            return;
        }

        var filePath = GetAbsoluteFilePath(imageAsset);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        db.ImageAssets.Remove(imageAsset);
        await db.SaveChangesAsync(cancellationToken);
    }

    public string GetAbsoluteFilePath(ImageAsset imageAsset) =>
        Path.Combine(_appPaths.DataRoot, imageAsset.StoredRelativePath);

    private string ResolveOwnerFolder(ImageOwnerType ownerType, int ownerId) => ownerType switch
    {
        ImageOwnerType.Campaign => _appPaths.CampaignImagesFolder(ownerId),
        ImageOwnerType.Monster => _appPaths.MonsterFolder(ownerId),
        _ => throw new ArgumentOutOfRangeException(nameof(ownerType), ownerType, null),
    };
}
