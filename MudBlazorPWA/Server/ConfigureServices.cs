using Microsoft.EntityFrameworkCore;
using MudBlazorPWA.Shared.Data;

namespace MudBlazorPWA.Server;
public static class ConfigureServices
{
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
