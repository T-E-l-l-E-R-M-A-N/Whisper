using System.Collections;
using Microsoft.EntityFrameworkCore;
using Whisper.Backend.ChatModels;
using Whisper.Backend.Database;

namespace Whisper.Backend.Base;

public class ServerBase
{
    private readonly MessengerDbContext _messengerDbContext;
    private List<ConnectionModel> _connections = new();
    public ServerBase(MessengerDbContext messengerDbContext)
    {
        _messengerDbContext = messengerDbContext;
    }
    public async Task ConnectAsync(long userId)
    {
        var user = await _messengerDbContext.Users.FirstOrDefaultAsync(x => x.LongId == userId);
        user!.Online = 1;
        _messengerDbContext.Users.Update(user);
        await _messengerDbContext.SaveChangesAsync();
        if (_connections.FirstOrDefault(x => x.User.LongId == userId) is { } connectionModel)
        {
            connectionModel.User.Online = 1;
        }
        else
        {
            _connections.Add(new ConnectionModel()
            {
                User = user
            });
        }
    }
    public async Task DisconnectAsync(long userId)
    {
        if (_connections.FirstOrDefault(x => x.User.LongId == userId) is { } connectionModel)
        {
            var user = await _messengerDbContext.Users.FirstOrDefaultAsync(x => x.LongId == userId);
            user!.Online = 0;
            _messengerDbContext.Users.Update(user);
            await _messengerDbContext.SaveChangesAsync();
            _connections.Remove(connectionModel);
        }
    }
    public async Task<IEnumerable<MessengerUserModel>> GetUsersAsync()
    {
        var users = await _messengerDbContext.Users.ToListAsync();
        
        return users;
    }
    public async Task<IEnumerable<MessengerRoomModel>> GetRoomsAsync(long userId)
    {
        var rooms = await _messengerDbContext.Rooms.Where(x =>
            x.Users.FindIndex(d => d.LongId == userId) != 0 ||
            x.CreatorId == userId).ToListAsync();
        return rooms;
    }
    public async Task<IEnumerable<MessengerMessageModel>> GetMessagesAsync(long roomId)
    {
        var messages = await _messengerDbContext.Messages.Where(x =>
            x.RoomId == roomId).ToListAsync();
        return messages;
    }
    public async Task<MessengerUserModel> GetUserByIdAsync(long userId)
    {
        var user = await _messengerDbContext.Users.FirstOrDefaultAsync(x => x.LongId == userId);
        return user!;
    }
    public async Task<MessengerRoomModel> GetRoomByIdAsync(long roomId)
    {
        var room = await _messengerDbContext.Rooms.FirstOrDefaultAsync(x => x.Id == roomId);
        return room!;
    }
    public async Task<MessengerMessageModel> GetMessageByIdAsync(long messageId)
    {
        var msg = await _messengerDbContext.Messages.FirstOrDefaultAsync(x => x.Id == messageId);
        return msg!;
    }
    public async Task<MessengerRoomModel> CreateRoomAsync(long creatorId, long[] users, string? name = "",
        List<MessengerMessageModel>? messages = null)
    {
        var room = new MessengerRoomModel()
        {
            CreatorId = creatorId,
            Id = Random.Shared.NextInt64(),
            Users = await _messengerDbContext.Users.Where(x => users.Contains(x.LongId)).ToListAsync()
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
                await _messengerDbContext.Messages.AddAsync(msg);
            }
        }

        await _messengerDbContext.SaveChangesAsync();

        return room;
    }

    public async Task<MessengerMessageModel> SendToBuddieAsync(long userId, long roomId, long targetId, string msg)
    {
        var room = await GetRoomByIdAsync(roomId);
        if (room == null)
        {

            string roomName = string.Join(" : ", userId.ToString(), targetId.ToString());
            room = await CreateRoomAsync(userId, new long[] { targetId }, roomName);
        }

        var message = new MessengerMessageModel()
        {
            Id = Random.Shared.NextInt64(),
            SenderId = userId,
            TargetId = targetId,
            RoomId = room.Id,
            Text = msg
        };
        
        await _messengerDbContext.Messages.AddAsync(message);

        await _messengerDbContext.SaveChangesAsync();

        return message;
    }

    public async Task<MessengerMessageModel> SendToGroupChatAsync(long userId, long roomId, string msg)
    {
        var room = await GetRoomByIdAsync(roomId);

        var message = new MessengerMessageModel()
        {
            Id = Random.Shared.NextInt64(),
            SenderId = userId,
            TargetId = roomId,
            RoomId = room.Id,
            Text = msg
        };
        
        await _messengerDbContext.Messages.AddAsync(message);

        await _messengerDbContext.SaveChangesAsync();

        return message;
    }

    public async Task<ConnectionModel> GetConnectionAsync(long userId)
    {
        var d = _connections.FirstOrDefault(x => x.User.LongId == userId);
        return d!;
    }
}