using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using ReactiveUI;

namespace Whisper.Frontend.Avalonia.Core;

public class MainViewModel : BaseViewModel
{
    private readonly IPageViewModelFactory _pageViewModelFactory;
    private readonly MessengerApiHelper _messengerApiHelper;

    [Reactive] public ObservableCollection<IPageViewModel> NavMenuCollection { get; set; } = new();

    [Reactive] public ObservableCollection<IPageViewModel> CurrentPageViewModel { get; set; } = new();

    public MainViewModel(
        IPageViewModelFactory pageViewModelFactory, 
        MessengerApiHelper messengerApiHelper)
    {
        _pageViewModelFactory = pageViewModelFactory;
        _messengerApiHelper = messengerApiHelper;

        GoBackCommand = ReactiveCommand.Create(() => CurrentPageViewModel.RemoveAt(0),
            this.WhenAnyValue(x => x.CurrentPageViewModel, value => value.Count != 1 && value.Count != 0));

        NavMenuOpenPageCommand = ReactiveCommand.Create<IPageViewModel>((x) => ChangePageViewModel(x));
    }

    [Reactive] public ReactiveCommand<Unit, Unit> GoBackCommand { get; set; }
    [Reactive] public ReactiveCommand<IPageViewModel, Unit> NavMenuOpenPageCommand { get; set; }

    public async void Init()
    {
        if (!await _messengerApiHelper.CheckTokenAndLogin())
        {
            CurrentPageViewModel.Add(await _pageViewModelFactory.GetWelcomePageViewModel());
            (CurrentPageViewModel.First() as WelcomePageViewModel)!.LoginComplete += OnLoginComplete;
            return;
        }

        await SwitchToHomePage();


    }

    public async void AppClose()
    {
        await _messengerApiHelper.DisconnectAsync();
    }

    private async void OnLoginComplete(object? sender, EventArgs e)
    {
        (sender as WelcomePageViewModel).LoginComplete -= OnLoginComplete;
        await Task.Delay(5000);

        await SwitchToHomePage();

        NavMenuCollection.Add(CurrentPageViewModel[0]);
        NavMenuCollection.Add(await _pageViewModelFactory.GetPeoplePageViewModel());
        NavMenuCollection.Add(await _pageViewModelFactory.GetRoomsPageViewModel());
        //NavMenuCollection.Add(await _pageViewModelFactory.GetSettingsPageViewModel());
    }

    private async Task SwitchToHomePage()
    {
        CurrentPageViewModel.Clear();
        CurrentPageViewModel.Add(await _pageViewModelFactory.GetProfilePageViewModel(_messengerApiHelper.UserId));
    }

    public void ChangePageViewModel(IPageViewModel pageViewModel)
    {
        if (CurrentPageViewModel[0] == pageViewModel)
            return;

        CurrentPageViewModel.Insert(0, pageViewModel);
    }
}