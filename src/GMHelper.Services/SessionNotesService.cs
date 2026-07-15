using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Data;
using Microsoft.EntityFrameworkCore;

namespace GMHelper.Services;

public class SessionNotesService : ISessionNotesService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public SessionNotesService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<SessionNote> CreateNoteAsync(int campaignId, string title, DateTime sessionDate, string markdownContent, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var note = new SessionNote
        {
            CampaignId = campaignId,
            Title = title,
            SessionDate = sessionDate,
            MarkdownContent = markdownContent,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.SessionNotes.Add(note);
        await db.SaveChangesAsync(cancellationToken);

        return note;
    }

    public async Task<IReadOnlyList<SessionNote>> GetNotesForCampaignAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.SessionNotes
            .AsNoTracking()
            .Where(n => n.CampaignId == campaignId)
            .OrderByDescending(n => n.SessionDate)
            .ThenByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateNoteAsync(int noteId, string title, DateTime sessionDate, string markdownContent, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var note = await db.SessionNotes.FindAsync([noteId], cancellationToken);
        if (note is null)
        {
            return;
        }

        note.Title = title;
        note.SessionDate = sessionDate;
        note.MarkdownContent = markdownContent;
        note.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteNoteAsync(int noteId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var note = await db.SessionNotes.FindAsync([noteId], cancellationToken);
        if (note is null)
        {
            return;
        }

        db.SessionNotes.Remove(note);
        await db.SaveChangesAsync(cancellationToken);
    }
}
