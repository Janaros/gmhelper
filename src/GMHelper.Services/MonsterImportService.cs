using System.Globalization;
using System.Text.Json;
using CsvHelper;
using GMHelper.Core.Abstractions;
using GMHelper.Core.Enums;
using GMHelper.Core.Models;

namespace GMHelper.Services;

public class MonsterImportService : IMonsterImportService
{
    private static readonly string[] ReservedCsvColumns = ["Name", "Notes", "ImagePath"];

    private readonly IMonsterService _monsterService;
    private readonly IStatFieldService _statFieldService;
    private readonly IImageLibraryService _imageLibraryService;

    public MonsterImportService(IMonsterService monsterService, IStatFieldService statFieldService, IImageLibraryService imageLibraryService)
    {
        _monsterService = monsterService;
        _statFieldService = statFieldService;
        _imageLibraryService = imageLibraryService;
    }

    public async Task<IReadOnlyList<MonsterImportRecord>> ParseJsonAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(filePath);
        var records = await JsonSerializer.DeserializeAsync<List<MonsterImportRecord>>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken);

        return records ?? [];
    }

    public Task<IReadOnlyList<MonsterImportRecord>> ParseCsvAsync(string filePath, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? [];
        var statColumns = headers.Where(h => !ReservedCsvColumns.Contains(h, StringComparer.OrdinalIgnoreCase)).ToList();

        var records = new List<MonsterImportRecord>();
        while (csv.Read())
        {
            var name = csv.GetField("Name");
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var record = new MonsterImportRecord
            {
                Name = name.Trim(),
                Notes = csv.TryGetField("Notes", out string? notes) ? notes : null,
                ImagePath = csv.TryGetField("ImagePath", out string? imagePath) ? imagePath : null,
            };

            foreach (var column in statColumns)
            {
                if (csv.TryGetField(column, out string? value) && !string.IsNullOrWhiteSpace(value))
                {
                    record.Stats.Add(new MonsterImportStatField { Name = column, Value = value });
                }
            }

            records.Add(record);
        }

        return Task.FromResult<IReadOnlyList<MonsterImportRecord>>(records);
    }

    public async Task<MonsterImportResult> CommitImportAsync(
        IReadOnlyList<MonsterImportRecord> records,
        string baseDirectory,
        MonsterImportConflictStrategy conflictStrategy,
        CancellationToken cancellationToken = default)
    {
        var result = new MonsterImportResult();
        var existingMonsters = await _monsterService.GetMonstersAsync(cancellationToken);

        foreach (var record in records)
        {
            var existing = existingMonsters.FirstOrDefault(m => string.Equals(m.Name, record.Name, StringComparison.OrdinalIgnoreCase));

            int monsterId;
            if (existing is null)
            {
                monsterId = (await _monsterService.CreateMonsterAsync(record.Name, cancellationToken)).Id;
                result.CreatedCount++;
            }
            else if (conflictStrategy == MonsterImportConflictStrategy.Skip)
            {
                result.SkippedCount++;
                continue;
            }
            else if (conflictStrategy == MonsterImportConflictStrategy.CreateDuplicate)
            {
                monsterId = (await _monsterService.CreateMonsterAsync(record.Name, cancellationToken)).Id;
                result.CreatedCount++;
            }
            else
            {
                monsterId = existing.Id;
                result.UpdatedCount++;
            }

            int? imageAssetId = existing?.ImageAssetId;
            if (!string.IsNullOrWhiteSpace(record.ImagePath))
            {
                var absoluteImagePath = Path.IsPathRooted(record.ImagePath)
                    ? record.ImagePath
                    : Path.Combine(baseDirectory, record.ImagePath);

                if (File.Exists(absoluteImagePath))
                {
                    var image = await _imageLibraryService.AddImageAsync(
                        Core.Enums.ImageOwnerType.Monster, monsterId, absoluteImagePath, Core.Enums.ImageCategory.Monster, cancellationToken);
                    imageAssetId = image.Id;
                }
            }

            await _monsterService.UpdateMonsterAsync(monsterId, record.Name, record.Notes, imageAssetId, cancellationToken);

            var fields = record.Stats.Select(s => (s.Name, s.Value)).ToList();
            await _statFieldService.ReplaceStatFieldsAsync(StatFieldOwnerType.Monster, monsterId, fields, cancellationToken);
        }

        return result;
    }
}
