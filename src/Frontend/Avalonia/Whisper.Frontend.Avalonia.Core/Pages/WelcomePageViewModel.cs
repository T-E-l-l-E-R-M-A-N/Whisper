using System.Reactive;
using System.Windows.Input;
using ReactiveUI;

namespace Whisper.Frontend.Avalonia.Core;

public sealed class WelcomePageViewModel : PageViewModelBase
{
    private readonly MessengerApiHelper _messengerApiHelper;

    [Reactive]
    public bool IsRegister { get; set; }
    [Reactive]
    public string Greeting { get; set; }
    [Reactive]
    public string Username { get; set; }
    [Reactive]
    public string Login { get; set; }
    [Reactive]
    public string Password { get; set; }
    
    public event EventHandler LoginComplete;
    
    public WelcomePageViewModel(MessengerApiHelper messengerApiHelper) : base(PageType.Welcome)
    {
        _messengerApiHelper = messengerApiHelper;
    }

    public void Init()
    {
        Greeting = "Welcome to Messenger";
    }

    public ReactiveCommand<Unit, Task> RegisterCommand => ReactiveCommand.Create(
        async () =>
        {
            var result = await _messengerApiHelper.AuthorizeAsync(new string[]
            {
                Username,
                Login,
                Password
            });

            if (result) LoginComplete?.Invoke(this, EventArgs.Empty);
        });
    public ReactiveCommand<Unit, Task> LoginCommand => ReactiveCommand.Create(
        async () =>
        {
            var result = await _messengerApiHelper.AuthorizeAsync(new string[]
            {
                Login,
                Password
            });

            if (result) LoginComplete?.Invoke(this, EventArgs.Empty);
        });
    public ReactiveCommand<Unit, bool> SwitchModeCommand => ReactiveCommand.Create(
        () => IsRegister = !IsRegister);
}