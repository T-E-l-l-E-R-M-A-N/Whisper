using Prism.Mvvm;

namespace Whisper.Frontend.Avalonia.Core;

public sealed class RoomViewModel : BindableBase
{
    public string Name { get; set; }
    public MessageViewModel LastMessage { get; set; }
}