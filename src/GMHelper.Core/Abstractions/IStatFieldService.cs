using GMHelper.Core.Entities;
using GMHelper.Core.Enums;

namespace GMHelper.Core.Abstractions;

/// <summary>
/// Manages the flexible key-value stat fields shared by Player and Monster (see <see cref="StatField"/>).
/// </summary>
public interface IStatFieldService
{
    Task<IReadOnlyList<StatField>> GetStatFieldsAsync(StatFieldOwnerType ownerType, int ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the entire stat field set for an owner with <paramref name="fields"/>, in order
    /// (SortOrder = position in the list). Editors work on the whole set at once, so a full
    /// replace is simpler and less error-prone than diffing individual add/remove/reorder calls.
    /// </summary>
    Task ReplaceStatFieldsAsync(StatFieldOwnerType ownerType, int ownerId, IReadOnlyList<(string Name, string Value)> fields, CancellationToken cancellationToken = default);
}
