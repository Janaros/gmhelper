using CommunityToolkit.Mvvm.ComponentModel;
using GMHelper.Core.Entities;

namespace GMHelper.App.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly CampaignListViewModel _campaignListViewModel;
    private readonly ICampaignDetailViewModelFactory _detailFactory;

    [ObservableProperty]
    private ObservableObject _currentViewModel;

    public ShellViewModel(CampaignListViewModel campaignListViewModel, ICampaignDetailViewModelFactory detailFactory)
    {
        _campaignListViewModel = campaignListViewModel;
        _detailFactory = detailFactory;

        _campaignListViewModel.CampaignOpened += OnCampaignOpened;

        _currentViewModel = _campaignListViewModel;
    }

    public async Task InitializeAsync()
    {
        await _campaignListViewModel.InitializeAsync();
    }

    private void OnCampaignOpened(object? sender, Campaign campaign)
    {
        var detail = _detailFactory.Create(campaign);
        detail.BackRequested += OnDetailBackRequested;
        CurrentViewModel = detail;
    }

    private void OnDetailBackRequested(object? sender, EventArgs e)
    {
        if (sender is CampaignDetailViewModel detail)
        {
            detail.BackRequested -= OnDetailBackRequested;
        }

        CurrentViewModel = _campaignListViewModel;
    }
}
