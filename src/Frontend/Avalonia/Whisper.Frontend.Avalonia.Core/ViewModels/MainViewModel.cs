using Prism.Mvvm;

namespace Whisper.Frontend.Avalonia.Core;

public class MainViewModel : BindableBase
{
    private readonly IPageViewModelFactory _pageViewModelFactory;
    private readonly MessengerApiHelper _messengerApiHelper;

    public MainViewModel(
        IPageViewModelFactory pageViewModelFactory, 
        MessengerApiHelper messengerApiHelper)
    {
        _pageViewModelFactory = pageViewModelFactory;
        _messengerApiHelper = messengerApiHelper;
    }
}