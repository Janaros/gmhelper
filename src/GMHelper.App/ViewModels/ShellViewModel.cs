using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMHelper.Core.Entities;

namespace GMHelper.App.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly CampaignListViewModel _campaignListViewModel;
    private readonly MonsterDatabaseViewModel _monsterDatabaseViewModel;
    private readonly ICampaignDetailViewModelFactory _detailFactory;

    [ObservableProperty]
    private ObservableObject _currentViewModel;

    /// <summary>Drives which nav-rail item is highlighted. Campaign list and campaign detail
    /// both count as the "Campaigns" section.</summary>
    [ObservableProperty]
    private bool _isMonsterDatabaseSection;

    public bool IsCampaignsSection => !IsMonsterDatabaseSection;

    partial void OnIsMonsterDatabaseSectionChanged(bool value) => OnPropertyChanged(nameof(IsCampaignsSection));

    public ShellViewModel(
        CampaignListViewModel campaignListViewModel,
        MonsterDatabaseViewModel monsterDatabaseViewModel,
        ICampaignDetailViewModelFactory detailFactory)
    {
        _campaignListViewModel = campaignListViewModel;
        _monsterDatabaseViewModel = monsterDatabaseViewModel;
        _detailFactory = detailFactory;

        _campaignListViewModel.CampaignOpened += OnCampaignOpened;
        _monsterDatabaseViewModel.BackRequested += (_, _) => NavigateToCampaigns();

        _currentViewModel = _campaignListViewModel;
    }

    public async Task InitializeAsync()
    {
        await _campaignListViewModel.InitializeAsync();
    }

    [RelayCommand]
    private void NavigateToCampaigns()
    {
        CurrentViewModel = _campaignListViewModel;
        IsMonsterDatabaseSection = false;
    }

    [RelayCommand]
    private async Task NavigateToMonsterDatabaseAsync()
    {
        CurrentViewModel = _monsterDatabaseViewModel;
        IsMonsterDatabaseSection = true;
        await _monsterDatabaseViewModel.InitializeAsync();
    }

    private async void OnCampaignOpened(object? sender, Campaign campaign)
    {
        var detail = _detailFactory.Create(campaign);
        detail.BackRequested += OnDetailBackRequested;
        CurrentViewModel = detail;
        IsMonsterDatabaseSection = false;

        await detail.InitializeAsync();
    }

    private void OnDetailBackRequested(object? sender, EventArgs e)
    {
        if (sender is CampaignDetailViewModel detail)
        {
            detail.BackRequested -= OnDetailBackRequested;
        }

        NavigateToCampaigns();
    }
}
