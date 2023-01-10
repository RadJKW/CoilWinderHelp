using CoilWinderHelp.Components.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CoilWinderHelp.Components.Extensions;
public static class ServicesExtension
{
  public static void TryAddCwHelpServices(this IServiceCollection services)
  {
    services.AddScoped<LayoutService>();
  }

}