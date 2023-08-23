using Prism.Mvvm;

namespace Whisper.Frontend.Avalonia.Core.ViewModels;

public sealed class MainViewModel : BindableBase
{
    private readonly IPageViewModelFactory _pageViewModelFactory;
    private readonly MessengerService _messengerService;

    public MainViewModel(
        IPageViewModelFactory pageViewModelFactory, 
        MessengerService messengerService)
    {
        _pageViewModelFactory = pageViewModelFactory;
        _messengerService = messengerService;
    }
}

public class MessengerService
{
}