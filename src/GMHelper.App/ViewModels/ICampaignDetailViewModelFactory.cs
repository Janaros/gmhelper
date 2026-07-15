using CommunityToolkit.Mvvm.Messaging;
using GMHelper.App.Windows;
using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using Microsoft.Extensions.Logging;

namespace GMHelper.App.ViewModels;

public interface ICampaignDetailViewModelFactory
{
    CampaignDetailViewModel Create(Campaign campaign);
}

public class CampaignDetailViewModelFactory : ICampaignDetailViewModelFactory
{
    private readonly IPdfLibraryService _pdfLibraryService;
    private readonly IImageLibraryService _imageLibraryService;
    private readonly IPlayerService _playerService;
    private readonly IStatFieldService _statFieldService;
    private readonly ICombatTrackerService _combatTrackerService;
    private readonly IMonsterService _monsterService;
    private readonly IMessenger _messenger;
    private readonly IWindowManager _windowManager;
    private readonly ILoggerFactory _loggerFactory;

    public CampaignDetailViewModelFactory(
        IPdfLibraryService pdfLibraryService,
        IImageLibraryService imageLibraryService,
        IPlayerService playerService,
        IStatFieldService statFieldService,
        ICombatTrackerService combatTrackerService,
        IMonsterService monsterService,
        IMessenger messenger,
        IWindowManager windowManager,
        ILoggerFactory loggerFactory)
    {
        _pdfLibraryService = pdfLibraryService;
        _imageLibraryService = imageLibraryService;
        _playerService = playerService;
        _statFieldService = statFieldService;
        _combatTrackerService = combatTrackerService;
        _monsterService = monsterService;
        _messenger = messenger;
        _windowManager = windowManager;
        _loggerFactory = loggerFactory;
    }

    public CampaignDetailViewModel Create(Campaign campaign)
    {
        var pdfLibrary = new PdfLibraryViewModel(
            campaign,
            _pdfLibraryService,
            _loggerFactory.CreateLogger<PdfLibraryViewModel>());

        var imageLibrary = new ImageLibraryViewModel(
            campaign,
            _imageLibraryService,
            _messenger,
            _windowManager,
            _loggerFactory.CreateLogger<ImageLibraryViewModel>());

        var roster = new RosterViewModel(
            campaign,
            _playerService,
            _statFieldService,
            _loggerFactory.CreateLogger<RosterViewModel>());

        var combatTracker = new CombatTrackerViewModel(
            campaign,
            _combatTrackerService,
            _monsterService,
            _loggerFactory.CreateLogger<CombatTrackerViewModel>());

        return new CampaignDetailViewModel(campaign, pdfLibrary, imageLibrary, roster, combatTracker);
    }
}
