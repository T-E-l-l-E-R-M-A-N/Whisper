namespace Whisper.Frontend.Avalonia.Core;

public sealed class RoomViewModel : BaseViewModel
{
    [Reactive] public long Id { get; set; }
    [Reactive] public string Name { get; set; }
    [Reactive] public MessageViewModel LastMessage { get; set; }
}