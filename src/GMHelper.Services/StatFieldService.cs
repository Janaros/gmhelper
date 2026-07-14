using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Core.Enums;
using GMHelper.Data;
using Microsoft.EntityFrameworkCore;

namespace GMHelper.Services;

public class StatFieldService : IStatFieldService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public StatFieldService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyList<StatField>> GetStatFieldsAsync(StatFieldOwnerType ownerType, int ownerId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.StatFields
            .AsNoTracking()
            .Where(s => s.OwnerType == ownerType && s.OwnerId == ownerId)
            .OrderBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceStatFieldsAsync(StatFieldOwnerType ownerType, int ownerId, IReadOnlyList<(string Name, string Value)> fields, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var existing = await db.StatFields
            .Where(s => s.OwnerType == ownerType && s.OwnerId == ownerId)
            .ToListAsync(cancellationToken);
        db.StatFields.RemoveRange(existing);

        for (var i = 0; i < fields.Count; i++)
        {
            db.StatFields.Add(new StatField
            {
                OwnerType = ownerType,
                OwnerId = ownerId,
                Name = fields[i].Name,
                Value = fields[i].Value,
                SortOrder = i,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
