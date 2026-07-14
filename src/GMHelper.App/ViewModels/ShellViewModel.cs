using CommunityToolkit.Mvvm.ComponentModel;
using GMHelper.Core.Entities;

namespace GMHelper.App.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly CampaignListViewModel _campaignListViewModel;
    private readonly MonsterDatabaseViewModel _monsterDatabaseViewModel;
    private readonly ICampaignDetailViewModelFactory _detailFactory;

    [ObservableProperty]
    private ObservableObject _currentViewModel;

    public ShellViewModel(
        CampaignListViewModel campaignListViewModel,
        MonsterDatabaseViewModel monsterDatabaseViewModel,
        ICampaignDetailViewModelFactory detailFactory)
    {
        _campaignListViewModel = campaignListViewModel;
        _monsterDatabaseViewModel = monsterDatabaseViewModel;
        _detailFactory = detailFactory;

        _campaignListViewModel.CampaignOpened += OnCampaignOpened;
        _campaignListViewModel.MonsterDatabaseRequested += OnMonsterDatabaseRequested;
        _monsterDatabaseViewModel.BackRequested += OnMonsterDatabaseBackRequested;

        _currentViewModel = _campaignListViewModel;
    }

    public async Task InitializeAsync()
    {
        await _campaignListViewModel.InitializeAsync();
    }

    private async void OnCampaignOpened(object? sender, Campaign campaign)
    {
        var detail = _detailFactory.Create(campaign);
        detail.BackRequested += OnDetailBackRequested;
        CurrentViewModel = detail;

        await detail.InitializeAsync();
    }

    private void OnDetailBackRequested(object? sender, EventArgs e)
    {
        if (sender is CampaignDetailViewModel detail)
        {
            detail.BackRequested -= OnDetailBackRequested;
        }

        CurrentViewModel = _campaignListViewModel;
    }

    private async void OnMonsterDatabaseRequested(object? sender, EventArgs e)
    {
        CurrentViewModel = _monsterDatabaseViewModel;
        await _monsterDatabaseViewModel.InitializeAsync();
    }

    private void OnMonsterDatabaseBackRequested(object? sender, EventArgs e)
    {
        CurrentViewModel = _campaignListViewModel;
    }
}
