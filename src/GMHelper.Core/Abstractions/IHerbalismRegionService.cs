using GMHelper.Core.Entities;

namespace GMHelper.Core.Abstractions;

public interface IHerbalismRegionService
{
    Task<IReadOnlyList<HerbalismRegion>> GetRegionsAsync(CancellationToken cancellationToken = default);
    Task<HerbalismRegion> CreateRegionAsync(string name, CancellationToken cancellationToken = default);
    Task UpdateRegionAsync(int regionId, string name, string? terrain, string? description, int difficultyClass, CancellationToken cancellationToken = default);

    /// <summary>Löscht das Gebiet samt seiner kompletten Fundtabelle.</summary>
    Task DeleteRegionAsync(int regionId, CancellationToken cancellationToken = default);
}
