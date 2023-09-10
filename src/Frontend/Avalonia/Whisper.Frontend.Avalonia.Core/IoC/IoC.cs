using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Whisper.Frontend.Avalonia.Core.Database;

namespace Whisper.Frontend.Avalonia.Core;

public static class IoC
{
    private static IServiceProvider _serviceProvider;

    public static async void Build(IServiceCollection? services = null)
    {
        if (services == null)
            services = new ServiceCollection();

        services.AddDbContext<FrontendDbContext>(options => options.UseSqlite("Data Source=frontend.db"), ServiceLifetime.Singleton);
        
        services.AddSingleton<MessengerApiHelper>();

        services.AddScoped<IPageViewModelFactory, PageViewModelFactory>();
        services.AddScoped<MainViewModel>();
        services.AddScoped<IPageViewModel, WelcomePageViewModel>();
        services.AddScoped<IPageViewModel, PeoplePageViewModel>();
        services.AddScoped<IPageViewModel, RoomsPageViewModel>();
        services.AddTransient<IPageViewModel, ProfilePageViewModel>();

        _serviceProvider = services.BuildServiceProvider();

        _serviceProvider.GetService<FrontendDbContext>().Database.EnsureCreated();
        await Resolve<MessengerApiHelper>().Init();

    }

    public static T Resolve<T>() => _serviceProvider.GetRequiredService<T>();
}