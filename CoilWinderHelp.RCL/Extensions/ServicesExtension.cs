using CoilWinderHelp.RCL.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace CoilWinderHelp.RCL.Extensions;
public static class ServicesExtension
{


  public static void TryAddCwHelpServices(this IServiceCollection services)
  {
    
    services.AddScoped<LayoutService>();

  }

}