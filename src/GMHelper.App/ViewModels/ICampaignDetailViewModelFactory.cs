using GMHelper.Core.Entities;

namespace GMHelper.App.ViewModels;

public interface ICampaignDetailViewModelFactory
{
    CampaignDetailViewModel Create(Campaign campaign);
}

public class CampaignDetailViewModelFactory : ICampaignDetailViewModelFactory
{
    public CampaignDetailViewModel Create(Campaign campaign) => new(campaign);
}
