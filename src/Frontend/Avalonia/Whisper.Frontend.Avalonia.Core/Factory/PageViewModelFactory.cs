namespace Whisper.Frontend.Avalonia.Core;

public class PageViewModelFactory : IPageViewModelFactory
{
    public async Task<IPageViewModel> GetWelcomePageViewModel()
    {
        var page = await GetPageViewModel(PageType.Welcome);
        (page as WelcomePageViewModel)?.Init();
        return page;
    }

    public async Task<IPageViewModel> GetRoomsPageViewModel()
    {
        var page = await GetPageViewModel(PageType.Rooms);
        await (page as RoomsPageViewModel)?.Init()!;
        return page;
    }
    public Task<IPageViewModel> GetRoomPageViewModel(long roomId) => null!;
    public async Task<IPageViewModel> GetPeoplePageViewModel()
    {
        var page = await GetPageViewModel(PageType.People);
        await (page as PeoplePageViewModel)?.Init()!;
        return page;
    }

    public async Task<IPageViewModel> GetProfilePageViewModel(long userId)
    {
        var page = await GetPageViewModel(PageType.Profile);
        await (page as ProfilePageViewModel)?.Init(userId)!;
        return page;
    }
    public Task<IPageViewModel> GetSettingsPageViewModel() => null!;
    public Task<IPageViewModel> GetSearchPageViewModel() => null!;

    private async Task<IPageViewModel> GetPageViewModel(PageType type)
    {
        return await Task.Run(() => IoC.Resolve<IEnumerable<IPageViewModel>>().FirstOrDefault(x => x.Type == type)!);
    }
}