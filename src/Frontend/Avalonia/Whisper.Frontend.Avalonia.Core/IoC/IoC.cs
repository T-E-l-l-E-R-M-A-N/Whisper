using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Whisper.Frontend.Avalonia.Core.Database;

namespace Whisper.Frontend.Avalonia.Core;

public static class IoC
{
    private static IServiceProvider _serviceProvider;

    public static void Build(IServiceCollection? services = null)
    {
        if (services == null)
            services = new ServiceCollection();

        services.AddDbContext<FrontendDbContext>(options => options.UseSqlite("Data Source=frontend.db"), ServiceLifetime.Singleton);
        
        services.AddSingleton<MessengerApiHelper>();

        services.AddScoped<IPageViewModelFactory, PageViewModelFactory>();
        services.AddScoped<MainViewModel>();
        
    }
}