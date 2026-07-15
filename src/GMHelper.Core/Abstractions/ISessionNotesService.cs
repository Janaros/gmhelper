using GMHelper.Core.Entities;

namespace GMHelper.Core.Abstractions;

public interface ISessionNotesService
{
    Task<SessionNote> CreateNoteAsync(int campaignId, string title, DateTime sessionDate, string markdownContent, CancellationToken cancellationToken = default);

    /// <summary>Ordered chronologically (most recent session first).</summary>
    Task<IReadOnlyList<SessionNote>> GetNotesForCampaignAsync(int campaignId, CancellationToken cancellationToken = default);

    Task UpdateNoteAsync(int noteId, string title, DateTime sessionDate, string markdownContent, CancellationToken cancellationToken = default);
    Task DeleteNoteAsync(int noteId, CancellationToken cancellationToken = default);
}
