using Microsoft.Extensions.DependencyInjection;
using Whisper.Backend.ChatModels;
using Whisper.Backend.Database;

namespace Whisper.Backend.Base;

public class ConnectionModel
{
    private ServerBase _srvBase;
    public MessengerUserModel User { get; set; }
    public long ActiveRoomId { get; set; }

    public event Action<MessengerMessageModel> NewMessageSent;

    #region Constructor

    public ConnectionModel(ServerBase srvBase, MessengerUserModel user)
    {
        User = user;
        _srvBase = srvBase;
    }

    #endregion

    public void Init(long activeRoomId)
    {
        ActiveRoomId = activeRoomId;

        using var scope = _srvBase.ServiceScopeFactory.CreateScope();

        var chatDbContext = scope.ServiceProvider.GetRequiredService<MessengerDbContext>();

        foreach (var chatMessage in chatDbContext.Messages)
            if(chatMessage.RoomId == ActiveRoomId) NewMessageSent?.Invoke(chatMessage);
    }

    public void Dispose()
    {
        _srvBase.Connections.Remove(this);
        _srvBase = null!;
    }

    public void MessageInvoked(MessengerMessageModel chatMessage)
    {
        NewMessageSent?.Invoke(chatMessage);
    }
}