using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using MudBlazorPWA.Shared.Data;
using MudBlazorPWA.Shared.Extensions;
using MudBlazorPWA.Shared.Models;
using MudExtensions.Services;

namespace MudBlazorPWA.Server;
public static class ConfigureServices
{
    public static void AddHostServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllersWithViews();
        services.AddRazorPages();
        services.AddMudServices();
        services.AddMudExtensions();
        services.AddDirectoryBrowser();
        services.AddLogging();
        services.AddLogging(loggingBuilder => {
            loggingBuilder.AddConsole();
        });

        services.AddCors(options => {
            options.AddPolicy(AppConfig.CorsPolicy,
            configurePolicy: b => b.AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyOrigin());
        });
        services.AddSignalR().AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new WindingCodeJsonConverter()));


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
