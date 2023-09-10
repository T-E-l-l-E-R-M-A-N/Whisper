using System.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Whisper.Backend.ChatModels;
using Whisper.Backend.Database;

namespace Whisper.Backend.Base;

public class ServerBase
{
    internal List<ConnectionModel> Connections = new();

    internal IServiceScopeFactory ServiceScopeFactory;

    

    public ServerBase(IServiceScopeFactory serviceScopeFactory)
    {
        ServiceScopeFactory = serviceScopeFactory;
    }

    public async void Init()
    {
        using var scope = ServiceScopeFactory.CreateScope();

        var messengerDbContext = scope.ServiceProvider.GetRequiredService<MessengerDbContext>();
        await messengerDbContext.Database.EnsureCreatedAsync();
    }
    public async Task ConnectAsync(long userId)
    {
        using var scope = ServiceScopeFactory.CreateScope();

        var messengerDbContext = scope.ServiceProvider.GetRequiredService<MessengerDbContext>();

        var user = await messengerDbContext.Users.FirstOrDefaultAsync(x => x.LongId == userId);
        user!.Online = 1;
        messengerDbContext.Users.Update(user);
        await messengerDbContext.SaveChangesAsync();
        if (Connections.FirstOrDefault(x => x.User.LongId == userId) is { } connectionModel)
        {
            connectionModel.User.Online = 1;
        }
        else
        {
            Connections.Add(new ConnectionModel(this, user));
        }
    }
    public async Task DisconnectAsync(long userId)
    {
        using var scope = ServiceScopeFactory.CreateScope();

        var messengerDbContext = scope.ServiceProvider.GetRequiredService<MessengerDbContext>();

        if (Connections.FirstOrDefault(x => x.User.LongId == userId) is { } connectionModel)
        {
            var user = await messengerDbContext.Users.FirstOrDefaultAsync(x => x.LongId == userId);
            user!.Online = 0;
            messengerDbContext.Users.Update(user);
            await messengerDbContext.SaveChangesAsync();
            Connections.Remove(connectionModel);
        }
    }
    public async Task<IEnumerable<MessengerUserModel>> GetUsersAsync()
    {
        using var scope = ServiceScopeFactory.CreateScope();

        var messengerDbContext = scope.ServiceProvider.GetRequiredService<MessengerDbContext>();

        var users = await messengerDbContext.Users.ToListAsync();
        
        return users;
    }
    public async Task<IEnumerable<MessengerRoomModel>> GetRoomsAsync(long userId)
    {
        using var scope = ServiceScopeFactory.CreateScope();

        var messengerDbContext = scope.ServiceProvider.GetRequiredService<MessengerDbContext>();

        List<MessengerRoomModel> rooms = new();
        await Task.Run(() =>
        {
            foreach (var room in messengerDbContext.Rooms)
            {
                if(room.Users.FindIndex(d => d.LongId == userId) != 0 ||
                   room.CreatorId == userId)
                    rooms.Add(room);
            }
        });
        return rooms;
    }
    public async Task<IEnumerable<MessengerMessageModel>> GetMessagesAsync(long roomId)
    {
        List<MessengerMessageModel> messages = new();
        await Task.Run(() =>
        {
            using var scope = ServiceScopeFactory.CreateScope();

            var messengerDbContext = scope.ServiceProvider.GetRequiredService<MessengerDbContext>();

            foreach (var msg in messengerDbContext.Messages)
            {
                if(msg.RoomId == roomId)
                    messages.Add(msg);
            }
        });
        return messages;
    }
    public async Task<MessengerUserModel> GetUserByIdAsync(long userId)
    {
        using var scope = ServiceScopeFactory.CreateScope();

        var messengerDbContext = scope.ServiceProvider.GetRequiredService<MessengerDbContext>();

        var user = await messengerDbContext.Users.FirstOrDefaultAsync(x => x.LongId == userId);
        return user!;
    }
    public async Task<MessengerRoomModel> GetRoomByIdAsync(long roomId)
    {
        using var scope = ServiceScopeFactory.CreateScope();

        var messengerDbContext = scope.ServiceProvider.GetRequiredService<MessengerDbContext>();

        var room = await messengerDbContext.Rooms.FirstOrDefaultAsync(x => x.Id == roomId);
        return room!;
    }
    public async Task<MessengerMessageModel> GetMessageByIdAsync(long messageId)
    {
        using var scope = ServiceScopeFactory.CreateScope();

        var messengerDbContext = scope.ServiceProvider.GetRequiredService<MessengerDbContext>();

        var msg = await messengerDbContext.Messages.FirstOrDefaultAsync(x => x.Id == messageId);
        return msg!;
    }
    public async Task<MessengerRoomModel> CreateRoomAsync(long creatorId, long[] users, string? name = "",
        List<MessengerMessageModel>? messages = null)
    {
        using var scope = ServiceScopeFactory.CreateScope();

        var messengerDbContext = scope.ServiceProvider.GetRequiredService<MessengerDbContext>();

        var room = new MessengerRoomModel()
        {
            Name = name,
            CreatorId = creatorId,
            Id = users.Length > 1 ? Random.Shared.NextInt64() : users[0],
            Users = await messengerDbContext.Users.Where(x => users.Contains(x.LongId)).ToListAsync()
        };

        if (string.IsNullOrEmpty(name))
        {
            string roomName = creatorId + " : " + string.Join(" : ", users);
        }
        
        if (messages != null && messages.Any())
        {
            foreach (var msg in messages)
            {
                msg.RoomId = room.Id;
                await messengerDbContext.Messages.AddAsync(msg);
            }
        }

        await messengerDbContext.Rooms.AddAsync(room);
        await messengerDbContext.SaveChangesAsync();

        return room;
    }

    public async Task<MessengerMessageModel> SendToAsync(long id, long userId, long roomId, long targetId, string msg)
    {
        using var scope = ServiceScopeFactory.CreateScope();

        var messengerDbContext = scope.ServiceProvider.GetRequiredService<MessengerDbContext>();
        if (roomId == 0)
        {
            roomId = targetId;
        }
        var room = await GetRoomByIdAsync(roomId);
        if (room == null)
        {

            string roomName = string.Join(" : ", userId.ToString(), targetId.ToString());
            room = await CreateRoomAsync(userId, new long[] { targetId }, roomName);
        }

        var message = new MessengerMessageModel()
        {
            Id = id,
            SenderId = userId,
            TargetId = targetId,
            RoomId = room.Id,
            Text = msg
        };

        foreach (var session in Connections)
        {
            session.MessageInvoked(message);
        }

        await messengerDbContext.Messages.AddAsync(message);

        await messengerDbContext.SaveChangesAsync();

        return message;
    }

    public async Task<MessengerMessageModel> SendToGroupChatAsync(long userId, long roomId, string msg)
    {
        using var scope = ServiceScopeFactory.CreateScope();

        var messengerDbContext = scope.ServiceProvider.GetRequiredService<MessengerDbContext>();

        var room = await GetRoomByIdAsync(roomId);

        var message = new MessengerMessageModel()
        {
            Id = Random.Shared.NextInt64(),
            SenderId = userId,
            TargetId = roomId,
            RoomId = room.Id,
            Text = msg
        };
        
        await messengerDbContext.Messages.AddAsync(message);

        await messengerDbContext.SaveChangesAsync();

        return message;
    }

    public async Task<ConnectionModel> GetConnectionAsync(long userId)
    {
        var d = Connections.FirstOrDefault(x => x.User.LongId == userId);
        return d!;
    }
}