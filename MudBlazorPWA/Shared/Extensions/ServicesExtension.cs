using MudBlazorPWA.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MudBlazorPWA.Shared.Extensions;
public static class ServicesExtension
{
  public static void TryAddCwHelpServices(this IServiceCollection services)
  {
    services.AddScoped<LayoutService>();
    services.AddScoped<DocViewService>();
  }

}
