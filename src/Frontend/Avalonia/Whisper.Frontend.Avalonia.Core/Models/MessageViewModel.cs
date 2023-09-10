namespace Whisper.Frontend.Avalonia.Core;

public sealed class MessageViewModel : BaseViewModel
{
    [Reactive] public long Id { get; set; }
    [Reactive] public string Sender { get; set; }
    [Reactive] public string Text { get; set; }
    [Reactive] public long RoomId { get; set; }
}