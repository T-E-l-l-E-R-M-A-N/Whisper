namespace Whisper.Backend.ChatModels;

public class MessengerRoomModel
{
    public long Id { get; set; }
    public string Name { get; set; }
    public List<MessengerUserModel> Users { get; set; }
    public long CreatorId { get; set; }
}