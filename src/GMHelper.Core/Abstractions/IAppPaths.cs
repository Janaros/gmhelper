namespace GMHelper.Core.Abstractions;

/// <summary>
/// Resolves the app's runtime data layout (database file, per-campaign folders, global library).
/// Injected so tests can point it at a temporary directory instead of the real data root.
/// </summary>
public interface IAppPaths
{
    string DataRoot { get; }
    string DatabaseFilePath { get; }
    string LogsFolder { get; }
    string CampaignsFolder { get; }
    string LibraryFolder { get; }

    string CampaignFolder(int campaignId);
    string CampaignPdfsFolder(int campaignId);
    string CampaignImagesFolder(int campaignId);
    string MonsterFolder(int monsterId);
}
