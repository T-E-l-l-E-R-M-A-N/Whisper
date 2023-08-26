using System.Windows.Input;
using Prism.Commands;

namespace Whisper.Frontend.Avalonia.Core;

public sealed class WelcomePageViewModel : PageViewModelBase
{
    private readonly MessengerApiHelper _messengerApiHelper;

    public bool IsRegister { get; set; }
    public string Greeting { get; set; }
    
    public string Username { get; set; }
    public string Login { get; set; }
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

    public ICommand RegisterCommand => new DelegateCommand(async () =>
    {
        var result = await _messengerApiHelper.AuthorizeAsync(new string[]
        {
            Username,
            Login,
            Password
        });
        
        if(result) LoginComplete?.Invoke(this, EventArgs.Empty);
    });

    public ICommand LoginCommand => new DelegateCommand(async () =>
    {
        var result = await _messengerApiHelper.AuthorizeAsync(new string[]
        {
            Login,
            Password
        });
        
        if(result) LoginComplete?.Invoke(this, EventArgs.Empty);
    });
    
    public ICommand SwitchModeCommand => new DelegateCommand(() =>
    {
        IsRegister = !IsRegister;
    });
}