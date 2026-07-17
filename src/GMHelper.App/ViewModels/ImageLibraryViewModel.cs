using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using GMHelper.App.Messaging;
using GMHelper.App.Windows;
using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Core.Enums;
using Microsoft.Extensions.Logging;

namespace GMHelper.App.ViewModels;

public partial class ImageLibraryViewModel : ObservableObject
{
    private readonly Campaign _campaign;
    private readonly IImageLibraryService _imageLibraryService;
    private readonly IMessenger _messenger;
    private readonly IWindowManager _windowManager;
    private readonly ILogger<ImageLibraryViewModel> _logger;

    public ObservableCollection<ImageAsset> Images { get; } = new();

    public IReadOnlyList<ImageCategory> Categories { get; } = Enum.GetValues<ImageCategory>();

    [ObservableProperty]
    private ImageAsset? _selectedImage;

    [ObservableProperty]
    private ImageCategory _selectedCategory = ImageCategory.Other;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isBlackedOut;

    public ImageLibraryViewModel(
        Campaign campaign,
        IImageLibraryService imageLibraryService,
        IMessenger messenger,
        IWindowManager windowManager,
        ILogger<ImageLibraryViewModel> logger)
    {
        _campaign = campaign;
        _imageLibraryService = imageLibraryService;
        _messenger = messenger;
        _windowManager = windowManager;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await ReloadAsync();
    }

    public async Task AddImageAsync(string sourceFilePath)
    {
        try
        {
            var added = await _imageLibraryService.AddImageAsync(ImageOwnerType.Campaign, _campaign.Id, sourceFilePath, SelectedCategory);
            StatusMessage = null;
            await ReloadAsync();
            SelectedImage = Images.FirstOrDefault(i => i.Id == added.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add image {SourceFilePath} to campaign {CampaignId}", sourceFilePath, _campaign.Id);
            StatusMessage = $"Fehler beim Hinzufügen: {ex.Message}";
        }
    }

    public string GetAbsoluteFilePath(ImageAsset imageAsset) => _imageLibraryService.GetAbsoluteFilePath(imageAsset);

    [RelayCommand]
    private async Task DeleteSelectedImageAsync()
    {
        if (SelectedImage is null)
        {
            return;
        }

        try
        {
            await _imageLibraryService.DeleteImageAsync(SelectedImage.Id);
            SelectedImage = null;
            StatusMessage = null;
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image {ImageAssetId}", SelectedImage?.Id);
            StatusMessage = $"Fehler beim Entfernen: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenSecondaryDisplay() => _windowManager.ShowSecondaryDisplay();

    [RelayCommand]
    private void ShowSelectedImageToPlayers()
    {
        if (SelectedImage is null)
        {
            return;
        }

        _windowManager.ShowSecondaryDisplay();
        IsBlackedOut = false;
        _messenger.Send(new ShowImageOnSecondaryDisplayMessage(GetAbsoluteFilePath(SelectedImage)));
    }

    partial void OnIsBlackedOutChanged(bool value) => _messenger.Send(new SetBlackoutMessage(value));

    private async Task ReloadAsync()
    {
        var images = await _imageLibraryService.GetImagesAsync(ImageOwnerType.Campaign, _campaign.Id);

        Images.Clear();
        foreach (var image in images)
        {
            Images.Add(image);
        }
    }
}
