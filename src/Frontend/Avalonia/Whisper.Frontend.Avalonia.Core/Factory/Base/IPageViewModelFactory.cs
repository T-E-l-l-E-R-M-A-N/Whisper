namespace Whisper.Frontend.Avalonia.Core;

public interface IPageViewModelFactory
{
    Task<IPageViewModel> GetWelcomePageViewModel();
    Task<IPageViewModel> GetRoomsPageViewModel();
    Task<IPageViewModel> GetRoomPageViewModel(long roomId);
    Task<IPageViewModel> GetPeoplePageViewModel();
    Task<IPageViewModel> GetProfilePageViewModel(long userId);
    Task<IPageViewModel> GetSettingsPageViewModel();
    Task<IPageViewModel> GetSearchPageViewModel();
}