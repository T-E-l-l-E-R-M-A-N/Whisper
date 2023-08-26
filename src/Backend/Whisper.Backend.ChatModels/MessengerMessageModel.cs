namespace Whisper.Backend.ChatModels;

public class MessengerMessageModel
{
    public long Id { get; set; }
    public string Text { get; set; }
    public long RoomId { get; set; }
    public long TargetId { get; set; }
    public long SenderId { get; set; }
}