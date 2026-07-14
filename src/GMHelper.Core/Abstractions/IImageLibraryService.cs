using GMHelper.Core.Entities;
using GMHelper.Core.Enums;

namespace GMHelper.Core.Abstractions;

public interface IImageLibraryService
{
    Task<ImageAsset> AddImageAsync(ImageOwnerType ownerType, int ownerId, string sourceFilePath, ImageCategory category, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ImageAsset>> GetImagesAsync(ImageOwnerType ownerType, int ownerId, CancellationToken cancellationToken = default);
    string GetAbsoluteFilePath(ImageAsset imageAsset);
}
