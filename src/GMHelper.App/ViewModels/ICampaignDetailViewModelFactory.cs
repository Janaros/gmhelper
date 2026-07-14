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
    private readonly ILoggerFactory _loggerFactory;

    public CampaignDetailViewModelFactory(IPdfLibraryService pdfLibraryService, ILoggerFactory loggerFactory)
    {
        _pdfLibraryService = pdfLibraryService;
        _loggerFactory = loggerFactory;
    }

    public CampaignDetailViewModel Create(Campaign campaign)
    {
        var pdfLibrary = new PdfLibraryViewModel(
            campaign,
            _pdfLibraryService,
            _loggerFactory.CreateLogger<PdfLibraryViewModel>());

        return new CampaignDetailViewModel(campaign, pdfLibrary);
    }
}
