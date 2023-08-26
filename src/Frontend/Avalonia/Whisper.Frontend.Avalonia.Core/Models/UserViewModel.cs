using Prism.Mvvm;

namespace Whisper.Frontend.Avalonia.Core;

public sealed class UserViewModel : BindableBase
{
    public string Name { get; set; }
    public bool Online { get; set; }
}