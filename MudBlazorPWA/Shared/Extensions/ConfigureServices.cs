using Blazored.LocalStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MudBlazorPWA.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazorPWA.Shared.Data;

namespace MudBlazorPWA.Shared.Extensions;
public static class ConfigureServices
{
    public static void AddClientServices(this IServiceCollection services)
    {
        services.AddBlazoredLocalStorage();
        services.AddScoped<LayoutService>();
        services.AddScoped<DocViewService>();
        services.AddScoped<HubClientService>();
    }
    public static void AddHostServices(this IServiceCollection services, IConfiguration configuration)
    {


        if (configuration.GetValue<bool>("UseInMemoryDatabase"))
        {
            services.AddDbContext<DataContext>(options =>
                options.UseInMemoryDatabase("InMemoryDb"));
        }
        else
        {
            // services.AddDbContext<DataContext>(options =>
            //     options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
            //     builder => builder.MigrationsAssembly(typeof(DataContext).Assembly.FullName)));
            services.AddDbContext<DataContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("SqLiteConnection"),
                builder => builder.MigrationsAssembly(typeof(DataContext).Assembly.FullName)));

        }


        services.AddScoped<IDataContext, DataContext>(provider => provider.GetRequiredService<DataContext>());
        services.AddScoped<DataContextInitializer>();
    }


}
