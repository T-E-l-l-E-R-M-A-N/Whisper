using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Whisper.Backend.Base;
using Whisper.Backend.ChatModels;
using Whisper.Shared.Protos;

namespace Whisper.Backend.Server;

[Authorize]
public class MessengerService : Messenger.MessengerBase
{
    private readonly ServerBase? _serverBase;

    public MessengerService(ServerBase? serverBase)
    {
        _serverBase = serverBase;
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

        while (!context.CancellationToken.IsCancellationRequested)
        {
            foreach (var msg in await _serverBase?.GetMessagesAsync(request.RoomId)!)
            {
                await responseStream.WriteAsync(new MessengerMessage()
                {
                    Id = msg.Id,
                    Message = msg.Text,
                    RoomId = msg.RoomId,
                    SenderId = msg.SenderId,
                    TargetId = msg.TargetId
                });
            }
            
            await Task.Delay(2000);
        }
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
        await _serverBase.SendToBuddieAsync(request.Message.SenderId, request.Message.RoomId, request.Message.TargetId, request.Message.Message);
        return request.Message;
    }
}