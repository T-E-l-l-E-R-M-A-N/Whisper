using Prism.Mvvm;

namespace Whisper.Frontend.Avalonia.Core;

public abstract class PageViewModelBase : BindableBase, IPageViewModel
{
    public string Header { get; set; }
    public PageType Type { get; set; }

    protected PageViewModelBase(PageType type)
    {
        Type = type;
    }
}