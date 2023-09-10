using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;

namespace Whisper.Frontend.Avalonia.Core;

public sealed class PeoplePageViewModel : PageViewModelBase
{
    private readonly MessengerApiHelper _messengerApiHelper;
    private readonly MainViewModel _mainViewModel;

    [Reactive] public ObservableCollection<UserViewModel> Users { get; set; } = new();
    [Reactive] public string NewMessageText { get; set; }
    [Reactive] public bool NewMessagePopupVisible { get; set; }
    [Reactive] public UserViewModel SelectedUserModel { get; set; }

    public PeoplePageViewModel(MessengerApiHelper messengerApiHelper, MainViewModel mainViewModel) : base(PageType.People)
    {
        _messengerApiHelper = messengerApiHelper;
        _mainViewModel = mainViewModel;

        SendNewMessageCommand = ReactiveCommand.Create(OnSendNewMessageToAsync);
        OpenPopupCommand = ReactiveCommand.Create<UserViewModel>((x) =>
        {
            SelectedUserModel = x;
            NewMessagePopupVisible = true;
        });
    }
    [Reactive] public ReactiveCommand<Unit, Unit> SendNewMessageCommand { get; set; }
    [Reactive] public ReactiveCommand<UserViewModel, Unit> OpenPopupCommand { get; set; }
    public async Task Init()
    {
        Header = "People";

        foreach (UserViewModel usr in await _messengerApiHelper.GetUsersAsync())
        {
            if(usr.Id == _messengerApiHelper.UserId)
                continue;

            Users.Add(usr);
        }
    }

    private async void OnSendNewMessageToAsync()
    {
        await _messengerApiHelper.SendMessageAsync(SelectedUserModel.Id, NewMessageText);
        NewMessageText = string.Empty;
        var roomsPaged = _mainViewModel.NavMenuCollection.First(x => x.Type == PageType.Rooms);
        _mainViewModel.ChangePageViewModel(roomsPaged);
        var roomsPage = _mainViewModel.CurrentPageViewModel[0] as RoomsPageViewModel;
        roomsPage!.SelectedRoomModel = roomsPage.Rooms.First(x => x.Id == SelectedUserModel.Id);
        SelectedUserModel = null!;
        NewMessagePopupVisible = false;
    }
}