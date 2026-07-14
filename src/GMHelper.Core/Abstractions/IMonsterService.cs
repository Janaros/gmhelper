using GMHelper.Core.Entities;

namespace GMHelper.Core.Abstractions;

public interface IMonsterService
{
    Task<Monster> CreateMonsterAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Monster>> GetMonstersAsync(CancellationToken cancellationToken = default);
    Task UpdateMonsterAsync(int monsterId, string name, string? notes, int? imageAssetId, CancellationToken cancellationToken = default);
    Task DeleteMonsterAsync(int monsterId, CancellationToken cancellationToken = default);
}
