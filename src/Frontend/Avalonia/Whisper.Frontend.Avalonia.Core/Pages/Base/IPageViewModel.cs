namespace Whisper.Frontend.Avalonia.Core;

public interface IPageViewModel
{
    string Header { get; set; }
    PageType Type { get; set; }
}