using System.Collections.ObjectModel;
using System.Reactive;
using System.Timers;
using Grpc.Core;
using ReactiveUI;
using Whisper.Shared.Protos;
using Timer = System.Timers.Timer;

namespace Whisper.Frontend.Avalonia.Core;

public sealed class RoomsPageViewModel : PageViewModelBase
{
    private readonly MessengerApiHelper _messengerApiHelper;
    private AsyncServerStreamingCall<MessengerMessage> _response;
    [Reactive] public ObservableCollection<RoomViewModel> Rooms { get; set; } = new();
    [Reactive] public ObservableCollection<MessageViewModel> Messages { get; set; } = new();
    [Reactive] public string NewMessageText { get; set; }
    [Reactive] public bool RoomVisible { get; set; }
    [Reactive] public RoomViewModel SelectedRoomModel { get; set; }
    public RoomsPageViewModel(MessengerApiHelper messengerApiHelper) : base(PageType.Rooms)
    {
        _messengerApiHelper = messengerApiHelper;

        OpenPopupCommand = ReactiveCommand.Create<RoomViewModel>(OnOpenPopup);

        ClosePopupCommand = ReactiveCommand.Create(() =>
        {
            SelectedRoomModel = null!;
            RoomVisible = false;
            _response?.Dispose();
        });
        SendNewMessageCommand = ReactiveCommand.Create(OnSendNewMessageToAsync);
    }

    private async void OnOpenPopup(RoomViewModel x)
    {
        SelectedRoomModel = x;
        RoomVisible = true;


        _response = _messengerApiHelper.GetMessagesDirect(new OpenRoomRequest()
        {
            RoomId = SelectedRoomModel.Id
        });

        var stream = _response.ResponseStream;

        try
        {
            await foreach (var msg in stream.ReadAllAsync())
            {
                if(Messages.FirstOrDefault(d=>d.Id == msg.Id) != null)
                    continue;

                var message = new MessageViewModel()
                {
                    Id = msg.Id,
                    Text = msg.Message,
                    Sender = (await _messengerApiHelper.GetUserByIdAsync(msg.SenderId)).Name,
                    RoomId = msg.RoomId

                };
                Messages.Add(message);
                SelectedRoomModel.LastMessage = message;
            }
        }
        catch
        {
            //ignored
        }

        //foreach (var msg in await _messengerApiHelper.GetMessagesAsync(SelectedRoomModel.Id))
        //{
        //    if (Messages.FirstOrDefault(x => x.Id == msg.Id) == null)
        //    {
        //        Messages.Add(msg);
        //        SelectedRoomModel.LastMessage = msg;
        //    }
        //}
    }

    [Reactive] public ReactiveCommand<RoomViewModel, Unit> OpenPopupCommand { get; set; }
    [Reactive] public ReactiveCommand<Unit, Unit> ClosePopupCommand { get; set; }
    [Reactive] public ReactiveCommand<Unit, Unit> SendNewMessageCommand { get; set; }
    public async Task Init()
    {
        Header = "My Rooms";

        _messengerApiHelper.NewMessageSent += MessengerApiHelperOnNewMessageSent;

        foreach (RoomViewModel room in await _messengerApiHelper.GetRoomsAsync())
        {

            Rooms.Add(room);
        }

        
    }
    private async void OnSendNewMessageToAsync()
    {
        await _messengerApiHelper.SendMessageAsync(SelectedRoomModel.Id, NewMessageText);
        NewMessageText = string.Empty;
    }

    private void MessengerApiHelperOnNewMessageSent(object? sender, MessageViewModel e)
    {
        try
        {
            var d = Rooms.First(x => x.Id == e.RoomId);
            var index = Rooms.IndexOf(d);
            Rooms.RemoveAt(index);
            d.LastMessage = e;
            Rooms.Insert(0, d);
        }
        catch
        {
            // ignored
        }
    }
}