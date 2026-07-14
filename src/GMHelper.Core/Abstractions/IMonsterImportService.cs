using GMHelper.Core.Enums;
using GMHelper.Core.Models;

namespace GMHelper.Core.Abstractions;

public interface IMonsterImportService
{
    Task<IReadOnlyList<MonsterImportRecord>> ParseJsonAsync(string filePath, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonsterImportRecord>> ParseCsvAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports <paramref name="records"/> into the monster database. <paramref name="baseDirectory"/>
    /// is the folder the import file itself lives in, used to resolve each record's relative ImagePath.
    /// </summary>
    Task<MonsterImportResult> CommitImportAsync(
        IReadOnlyList<MonsterImportRecord> records,
        string baseDirectory,
        MonsterImportConflictStrategy conflictStrategy,
        CancellationToken cancellationToken = default);
}
