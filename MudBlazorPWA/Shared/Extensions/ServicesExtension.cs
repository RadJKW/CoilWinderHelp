using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MudBlazorPWA.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazorPWA.Shared.Data;

namespace MudBlazorPWA.Shared.Extensions;
public static class ServicesExtension
{
  public static void AddClientServices(this IServiceCollection services)
  {
    services.AddScoped<LayoutService>();
    services.AddScoped<DocViewService>();
  }
  public static void AddHostServices(this IServiceCollection services, IConfiguration configuration)
  {

      services.AddDbContext<DataContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
          builder => builder.MigrationsAssembly(typeof(DataContext).Assembly.FullName)));

    services.AddDbContext<DataContext>(options =>
      options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
        builder => builder.MigrationsAssembly(typeof(DataContext).Assembly.FullName)));

    services.AddScoped<IDataContext>(provider => provider.GetRequiredService<DataContext>());
    services.AddScoped<DataContextInitializer>();
  }


}
