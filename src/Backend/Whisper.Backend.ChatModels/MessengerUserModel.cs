using Microsoft.AspNetCore.Identity;

namespace Whisper.Backend.ChatModels;

public class MessengerUserModel : IdentityUser
{
    public long LongId { get; set; }
    public string ScreenName { get; set; }
    public int Online { get; set; }
}