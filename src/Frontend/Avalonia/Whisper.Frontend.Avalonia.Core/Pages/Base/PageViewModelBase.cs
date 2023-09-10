namespace Whisper.Frontend.Avalonia.Core;

public abstract class PageViewModelBase : BaseViewModel, IPageViewModel
{
    [Reactive]
    public string Header { get; set; }
    [Reactive]
    public PageType Type { get; set; }

    protected PageViewModelBase(PageType type)
    {
        Type = type;
    }
}