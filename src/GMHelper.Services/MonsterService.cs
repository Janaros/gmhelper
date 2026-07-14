using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Data;
using Microsoft.EntityFrameworkCore;

namespace GMHelper.Services;

public class MonsterService : IMonsterService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IAppPaths _appPaths;

    public MonsterService(IDbContextFactory<AppDbContext> dbContextFactory, IAppPaths appPaths)
    {
        _dbContextFactory = dbContextFactory;
        _appPaths = appPaths;
    }

    public async Task<Monster> CreateMonsterAsync(string name, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var monster = new Monster
        {
            Name = name,
            Source = "Manual",
            CreatedAt = DateTime.UtcNow,
        };

        db.Monsters.Add(monster);
        await db.SaveChangesAsync(cancellationToken);

        return monster;
    }

    public async Task<IReadOnlyList<Monster>> GetMonstersAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Monsters
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateMonsterAsync(int monsterId, string name, string? notes, int? imageAssetId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var monster = await db.Monsters.FindAsync([monsterId], cancellationToken);
        if (monster is null)
        {
            return;
        }

        monster.Name = name;
        monster.Notes = notes;
        monster.ImageAssetId = imageAssetId;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteMonsterAsync(int monsterId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var monster = await db.Monsters.FindAsync([monsterId], cancellationToken);
        if (monster is null)
        {
            return;
        }

        db.Monsters.Remove(monster);
        await db.SaveChangesAsync(cancellationToken);

        var folder = _appPaths.MonsterFolder(monsterId);
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}
