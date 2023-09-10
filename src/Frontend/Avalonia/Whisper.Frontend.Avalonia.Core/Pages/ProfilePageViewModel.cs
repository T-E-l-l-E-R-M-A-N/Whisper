namespace Whisper.Frontend.Avalonia.Core;

public sealed class ProfilePageViewModel : PageViewModelBase
{
    private readonly MessengerApiHelper _messengerApiHelper;

    [Reactive] public UserViewModel User { get; set; }
    public ProfilePageViewModel(MessengerApiHelper messengerApiHelper) : base(PageType.Profile)
    {
        _messengerApiHelper = messengerApiHelper;
    }

    public async Task Init(long id)
    {
        User = await _messengerApiHelper.GetUserByIdAsync(id);
        Header = User.Name;
    }
}