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
    private readonly IMessenger _messenger;
    private readonly IWindowManager _windowManager;
    private readonly ILoggerFactory _loggerFactory;

    public CampaignDetailViewModelFactory(
        IPdfLibraryService pdfLibraryService,
        IImageLibraryService imageLibraryService,
        IMessenger messenger,
        IWindowManager windowManager,
        ILoggerFactory loggerFactory)
    {
        _pdfLibraryService = pdfLibraryService;
        _imageLibraryService = imageLibraryService;
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

        return new CampaignDetailViewModel(campaign, pdfLibrary, imageLibrary);
    }
}
