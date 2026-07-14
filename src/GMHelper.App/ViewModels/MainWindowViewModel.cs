using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using Microsoft.Extensions.Logging;

namespace GMHelper.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ICampaignService _campaignService;
    private readonly ILogger<MainWindowViewModel> _logger;

    public ObservableCollection<Campaign> Campaigns { get; } = new();

    [ObservableProperty]
    private string _newCampaignName = string.Empty;

    [ObservableProperty]
    private Campaign? _selectedCampaign;

    [ObservableProperty]
    private string? _statusMessage;

    public MainWindowViewModel(ICampaignService campaignService, ILogger<MainWindowViewModel> logger)
    {
        _campaignService = campaignService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await ReloadCampaignsAsync();
    }

    [RelayCommand]
    private async Task AddCampaignAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCampaignName))
        {
            return;
        }

        try
        {
            await _campaignService.CreateCampaignAsync(NewCampaignName.Trim(), description: null);
            NewCampaignName = string.Empty;
            StatusMessage = null;
            await ReloadCampaignsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create campaign {CampaignName}", NewCampaignName);
            StatusMessage = $"Fehler beim Anlegen: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteCampaignAsync()
    {
        if (SelectedCampaign is null)
        {
            return;
        }

        try
        {
            await _campaignService.DeleteCampaignAsync(SelectedCampaign.Id);
            SelectedCampaign = null;
            StatusMessage = null;
            await ReloadCampaignsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete campaign {CampaignId}", SelectedCampaign?.Id);
            StatusMessage = $"Fehler beim Löschen: {ex.Message}";
        }
    }

    private async Task ReloadCampaignsAsync()
    {
        var campaigns = await _campaignService.GetCampaignsAsync();

        Campaigns.Clear();
        foreach (var campaign in campaigns)
        {
            Campaigns.Add(campaign);
        }
    }
}
