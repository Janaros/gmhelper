using GMHelper.Core.Abstractions;

namespace GMHelper.Services;

/// <summary>
/// Default <see cref="IAppPaths"/> implementation rooted at a caller-supplied directory.
/// Production composition root passes the project's Data folder; tests pass a temp directory.
/// </summary>
public class AppPaths : IAppPaths
{
    public AppPaths(string dataRoot)
    {
        DataRoot = dataRoot;
    }

    public string DataRoot { get; }

    public string DatabaseFilePath => Path.Combine(DataRoot, "app.db");
    public string LogsFolder => Path.Combine(DataRoot, "logs");
    public string CampaignsFolder => Path.Combine(DataRoot, "Campaigns");
    public string LibraryFolder => Path.Combine(DataRoot, "Library");

    public string CampaignFolder(int campaignId) => Path.Combine(CampaignsFolder, campaignId.ToString());
    public string CampaignPdfsFolder(int campaignId) => Path.Combine(CampaignFolder(campaignId), "Pdfs");
    public string CampaignImagesFolder(int campaignId) => Path.Combine(CampaignFolder(campaignId), "Images");
    public string MonsterFolder(int monsterId) => Path.Combine(LibraryFolder, "Monsters", monsterId.ToString());
}
