using Prism.Mvvm;

namespace Whisper.Frontend.Avalonia.Core;

public class MainViewModel : BindableBase
{
    private readonly IPageViewModelFactory _pageViewModelFactory;
    private readonly MessengerApiHelper _messengerApiHelper;
    public IPageViewModel CurrentPageViewModel { get; set; }

    public MainViewModel(
        IPageViewModelFactory pageViewModelFactory, 
        MessengerApiHelper messengerApiHelper)
    {
        _pageViewModelFactory = pageViewModelFactory;
        _messengerApiHelper = messengerApiHelper;
    }

    public async void Init()
    {
        if (!await _messengerApiHelper.CheckTokenAndLogin())
        {
            CurrentPageViewModel = await _pageViewModelFactory.GetWelcomePageViewModel();
        }
    }
}