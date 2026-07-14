using System.Globalization;
using System.Text.Json;
using CsvHelper;
using GMHelper.Core.Abstractions;
using GMHelper.Core.Enums;
using GMHelper.Core.Models;

namespace GMHelper.Services;

public class MonsterExportService : IMonsterExportService
{
    private readonly IMonsterService _monsterService;
    private readonly IStatFieldService _statFieldService;
    private readonly IImageLibraryService _imageLibraryService;

    public MonsterExportService(IMonsterService monsterService, IStatFieldService statFieldService, IImageLibraryService imageLibraryService)
    {
        _monsterService = monsterService;
        _statFieldService = statFieldService;
        _imageLibraryService = imageLibraryService;
    }

    public async Task ExportToJsonAsync(string destinationFilePath, CancellationToken cancellationToken = default)
    {
        var records = await BuildExportRecordsAsync(destinationFilePath, cancellationToken);

        await using var stream = File.Create(destinationFilePath);
        await JsonSerializer.SerializeAsync(
            stream,
            records,
            new JsonSerializerOptions { WriteIndented = true },
            cancellationToken);
    }

    public async Task ExportToCsvAsync(string destinationFilePath, CancellationToken cancellationToken = default)
    {
        var records = await BuildExportRecordsAsync(destinationFilePath, cancellationToken);

        var statColumns = records
            .SelectMany(r => r.Stats.Select(s => s.Name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        using var writer = new StreamWriter(destinationFilePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteField("Name");
        csv.WriteField("Notes");
        csv.WriteField("ImagePath");
        foreach (var column in statColumns)
        {
            csv.WriteField(column);
        }
        csv.NextRecord();

        foreach (var record in records)
        {
            csv.WriteField(record.Name);
            csv.WriteField(record.Notes ?? string.Empty);
            csv.WriteField(record.ImagePath ?? string.Empty);

            foreach (var column in statColumns)
            {
                var value = record.Stats.FirstOrDefault(s => string.Equals(s.Name, column, StringComparison.OrdinalIgnoreCase))?.Value;
                csv.WriteField(value ?? string.Empty);
            }

            csv.NextRecord();
        }
    }

    private async Task<List<MonsterImportRecord>> BuildExportRecordsAsync(string destinationFilePath, CancellationToken cancellationToken)
    {
        var destinationDirectory = Path.GetDirectoryName(destinationFilePath)
            ?? throw new ArgumentException("Destination file path must include a directory.", nameof(destinationFilePath));
        var imagesFolder = Path.Combine(destinationDirectory, "Images");

        var monsters = await _monsterService.GetMonstersAsync(cancellationToken);
        var records = new List<MonsterImportRecord>();

        foreach (var monster in monsters)
        {
            var statFields = await _statFieldService.GetStatFieldsAsync(StatFieldOwnerType.Monster, monster.Id, cancellationToken);

            string? relativeImagePath = null;
            if (monster.ImageAssetId is { } imageAssetId)
            {
                var images = await _imageLibraryService.GetImagesAsync(ImageOwnerType.Monster, monster.Id, cancellationToken);
                var image = images.FirstOrDefault(i => i.Id == imageAssetId);
                if (image is not null)
                {
                    Directory.CreateDirectory(imagesFolder);
                    var destinationImagePath = FileNaming.ResolveUniqueDestinationPath(imagesFolder, image.FileName);
                    File.Copy(_imageLibraryService.GetAbsoluteFilePath(image), destinationImagePath);
                    relativeImagePath = Path.Combine("Images", Path.GetFileName(destinationImagePath));
                }
            }

            records.Add(new MonsterImportRecord
            {
                Name = monster.Name,
                Notes = monster.Notes,
                ImagePath = relativeImagePath,
                Stats = statFields.Select(s => new MonsterImportStatField { Name = s.Name, Value = s.Value }).ToList(),
            });
        }

        return records;
    }
}
