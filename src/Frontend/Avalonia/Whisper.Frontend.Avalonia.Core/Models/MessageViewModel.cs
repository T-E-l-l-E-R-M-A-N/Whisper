using Prism.Mvvm;

namespace Whisper.Frontend.Avalonia.Core;

public sealed class MessageViewModel : BindableBase
{
    public string Sender { get; set; }
    public string Text { get; set; }
}