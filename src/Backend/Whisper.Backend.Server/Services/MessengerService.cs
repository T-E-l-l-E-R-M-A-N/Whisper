using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Whisper.Backend.Base;
using Whisper.Backend.ChatModels;
using Whisper.Shared.Protos;

namespace Whisper.Backend.Server;

[Authorize]
public class MessengerService : Messenger.MessengerBase
{
    private readonly ServerBase? _serverBase;
    private readonly UserManager<MessengerUserModel> _userManager;

    private IServerStreamWriter<MessengerMessage> _messagesResponseStream;

    public MessengerService(ServerBase? serverBase, UserManager<MessengerUserModel> userManager)
    {
        _serverBase = serverBase;
        _userManager = userManager;
    }

    public override async Task<ConnectResponse> Connect(ConnectRequest request, ServerCallContext context)
    {
        await _serverBase?.ConnectAsync(request.UserId)!;
        return new ConnectResponse();
    }

    public override async Task<ConnectResponse> Disconnect(ConnectRequest request, ServerCallContext context)
    {
        await _serverBase?.DisconnectAsync(request.UserId)!;
        return new ConnectResponse();
    }

    public override async Task GetRooms(ConnectRequest request, IServerStreamWriter<MessengerRoom> responseStream, ServerCallContext context)
    {
        foreach (var room in await _serverBase?.GetRoomsAsync(request.UserId)!)
        {
            await responseStream.WriteAsync(new MessengerRoom()
            {
                Id = room.Id,
                Name = room.Name,
                Users = { room.Users.Select(x =>
                {
                    return new MessengerUser()
                    {
                        Id = x.LongId,
                        Name = x.ScreenName,
                        Online = _serverBase.GetConnectionAsync(x.LongId).Result != null
                    };
                }) }
            });
        }
    }

    public override async Task GetUsers(ConnectRequest request, IServerStreamWriter<MessengerUser> responseStream, ServerCallContext context)
    {
        foreach (var user in await _serverBase?.GetUsersAsync()!)
        {
            await responseStream.WriteAsync(new MessengerUser()
            {
                Id = user.LongId,
                Name = user.ScreenName,
                Online = (await _serverBase.GetConnectionAsync(user.LongId)) != null
            });
        }
    }

    public override async Task GetMessages(OpenRoomRequest request, IServerStreamWriter<MessengerMessage> responseStream, ServerCallContext context)
    {
        var user = await _userManager.GetUserAsync(context.GetHttpContext().User);

        var session = await _serverBase.GetConnectionAsync(user.LongId);

        _messagesResponseStream = responseStream;

        session.NewMessageSent += SessionOnNewMessageSent;

        session.Init(request.RoomId);

        try
        {
            await Task.Delay(int.MaxValue, context.CancellationToken);
        }
        catch (TaskCanceledException e)
        {
            Console.WriteLine(e.Message, "ChatRoomService.JoinChat.Cancelled");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message, "ChatRoomService.JoinChat");
        }
        finally
        {
            session.NewMessageSent -= SessionOnNewMessageSent;

            session.Dispose();
            _messagesResponseStream = null!;
        }
        _messagesResponseStream = responseStream;
    }


    public override async Task<MessengerRoom> CreateRoom(CreateRoomRequest request, ServerCallContext context)
    {
        var room = new MessengerRoom()
        {
            Id = Random.Shared.NextInt64(),
            Name = request.Name,
            Users = { request.Users }
        };

        await _serverBase.CreateRoomAsync(request.CreatorId, request.Users.Select(x => x.Id).ToArray(), room.Name,
            new()
            {
                new MessengerMessageModel()
                {
                    Id = Random.Shared.NextInt64(),
                    RoomId = room.Id,
                    SenderId = request.CreatorId,
                    TargetId = request.Users.FirstOrDefault()!.Id,
                }
            });

        return room;
    }

    public override async Task<MessengerMessage> SendTo(SendRequest request, ServerCallContext context)
    {
        await _serverBase.SendToAsync(request.Message.Id, request.Message.SenderId, request.Message.RoomId, request.Message.TargetId, request.Message.Message);
        return request.Message;
    }

    public override async Task<MessengerUser> GetUser(GetUserRequest request, ServerCallContext context)
    {
        var user = await _serverBase?.GetUserByIdAsync(request.UserId);

        return new MessengerUser()
        {
            Id = user.LongId,
            Name = user.ScreenName,
            Online = user.Online == 1
        };
    }
    public override async Task<MessengerMessage> GetMessage(GetMessageRequest request, ServerCallContext context)
    {
        var msg = await _serverBase?.GetMessageByIdAsync(request.Id)!;

        return new MessengerMessage()
        {
            Id = msg.Id,
            Message = msg.Text,
            RoomId = msg.RoomId,
            SenderId = msg.SenderId,
            TargetId = msg.TargetId
        };
    }


    private async void SessionOnNewMessageSent(MessengerMessageModel message)
    {
        await _messagesResponseStream.WriteAsync(new MessengerMessage()
        {
            Message = message.Text,
            Id = message.Id,
            RoomId = message.RoomId,
            SenderId = message.SenderId,
            TargetId = message.TargetId
        });
    }
}