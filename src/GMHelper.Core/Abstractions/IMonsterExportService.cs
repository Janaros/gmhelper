namespace GMHelper.Core.Abstractions;

public interface IMonsterExportService
{
    /// <summary>
    /// Writes all monsters to <paramref name="destinationFilePath"/> (JSON, same schema
    /// IMonsterImportService reads). Monster images are copied into an "Images" subfolder
    /// next to the destination file so the export folder is self-contained and re-importable.
    /// </summary>
    Task ExportToJsonAsync(string destinationFilePath, CancellationToken cancellationToken = default);

    /// <summary>Writes all monsters to a CSV file: Name, Notes, ImagePath, then one column per distinct stat name.</summary>
    Task ExportToCsvAsync(string destinationFilePath, CancellationToken cancellationToken = default);
}
