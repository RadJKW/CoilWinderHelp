using CoilWinderHelp.Pages.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CoilWinderHelp.Pages.Extensions;
public static class ServicesExtension
{
  public static void TryAddCwHelpServices(this IServiceCollection services)
  {
    services.AddScoped<LayoutService>();
  }

}