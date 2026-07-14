using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using GMHelper.App.Messaging;

namespace GMHelper.App.ViewModels;

/// <summary>
/// Player-facing view model for the secondary display window. Deliberately knows nothing
/// about campaigns, combat, initiative, or monster stats — it only ever receives an absolute
/// image path and a blackout flag via messages, so GM-only data can never leak onto this screen.
/// </summary>
public partial class SecondaryDisplayViewModel : ObservableObject,
    IRecipient<ShowImageOnSecondaryDisplayMessage>,
    IRecipient<SetBlackoutMessage>
{
    [ObservableProperty]
    private string? _imagePath;

    [ObservableProperty]
    private bool _isBlackedOut;

    public SecondaryDisplayViewModel(IMessenger messenger)
    {
        messenger.RegisterAll(this);
    }

    public void Receive(ShowImageOnSecondaryDisplayMessage message)
    {
        ImagePath = message.AbsoluteImagePath;
        IsBlackedOut = false;
    }

    public void Receive(SetBlackoutMessage message)
    {
        IsBlackedOut = message.IsBlackedOut;
    }
}
