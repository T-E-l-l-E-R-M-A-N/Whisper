namespace Whisper.Frontend.Avalonia.Core;

public sealed class UserViewModel : BaseViewModel
{
    [Reactive]
    public string Name { get; set; }
    [Reactive]
    public bool Online { get; set; }
    [Reactive]
    public long Id { get; set; }
}