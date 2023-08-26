namespace Whisper.Frontend.Avalonia.Core;

public class PageViewModelFactory : IPageViewModelFactory
{
    public async Task<IPageViewModel> GetWelcomePageViewModel()
    {
        var page = await GetPageViewModel(PageType.Welcome);
        (page as WelcomePageViewModel).Init();
        return page;
    }

    public Task<IPageViewModel> GetRoomsPageViewModel() => null;
    public Task<IPageViewModel> GetRoomPageViewModel(long roomId) => null;
    public Task<IPageViewModel> GetPeoplePageViewModel() => null;
    public Task<IPageViewModel> GetProfilePageViewModel(long userId) => null;
    public Task<IPageViewModel> GetSettingsPageViewModel() => null;
    public Task<IPageViewModel> GetSearchPageViewModel() => null;

    private async Task<IPageViewModel> GetPageViewModel(PageType type)
    {
        return IoC.Resolve<IEnumerable<IPageViewModel>>().FirstOrDefault(x => x.Type == type)!;
    }
}