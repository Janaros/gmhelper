using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMHelper.Core.Entities;

namespace GMHelper.App.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly CampaignListViewModel _campaignListViewModel;
    private readonly MonsterDatabaseViewModel _monsterDatabaseViewModel;
    private readonly HerbalismViewModel _herbalismViewModel;
    private readonly ICampaignDetailViewModelFactory _detailFactory;

    [ObservableProperty]
    private ObservableObject _currentViewModel;

    /// <summary>Drives which nav-rail item is highlighted. Campaign list and campaign detail
    /// both count as the "Campaigns" section.</summary>
    [ObservableProperty]
    private bool _isMonsterDatabaseSection;

    [ObservableProperty]
    private bool _isHerbalismSection;

    public bool IsCampaignsSection => !IsMonsterDatabaseSection && !IsHerbalismSection;

    partial void OnIsMonsterDatabaseSectionChanged(bool value) => OnPropertyChanged(nameof(IsCampaignsSection));

    partial void OnIsHerbalismSectionChanged(bool value) => OnPropertyChanged(nameof(IsCampaignsSection));

    public ShellViewModel(
        CampaignListViewModel campaignListViewModel,
        MonsterDatabaseViewModel monsterDatabaseViewModel,
        HerbalismViewModel herbalismViewModel,
        ICampaignDetailViewModelFactory detailFactory)
    {
        _campaignListViewModel = campaignListViewModel;
        _monsterDatabaseViewModel = monsterDatabaseViewModel;
        _herbalismViewModel = herbalismViewModel;
        _detailFactory = detailFactory;

        _campaignListViewModel.CampaignOpened += OnCampaignOpened;
        _monsterDatabaseViewModel.BackRequested += (_, _) => NavigateToCampaigns();
        _herbalismViewModel.BackRequested += (_, _) => NavigateToCampaigns();

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
        IsHerbalismSection = false;
    }

    [RelayCommand]
    private async Task NavigateToMonsterDatabaseAsync()
    {
        CurrentViewModel = _monsterDatabaseViewModel;
        IsMonsterDatabaseSection = true;
        IsHerbalismSection = false;
        await _monsterDatabaseViewModel.InitializeAsync();
    }

    [RelayCommand]
    private async Task NavigateToHerbalismAsync()
    {
        CurrentViewModel = _herbalismViewModel;
        IsMonsterDatabaseSection = false;
        IsHerbalismSection = true;
        await _herbalismViewModel.InitializeAsync();
    }

    private async void OnCampaignOpened(object? sender, Campaign campaign)
    {
        var detail = _detailFactory.Create(campaign);
        detail.BackRequested += OnDetailBackRequested;
        CurrentViewModel = detail;
        IsMonsterDatabaseSection = false;
        IsHerbalismSection = false;

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
