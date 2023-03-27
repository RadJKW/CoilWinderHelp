using Blazored.LocalStorage;
using MudBlazorPWA.Client.Services;

namespace MudBlazorPWA.Client;
public static class ConfigureServices
{
    public static void AddClientServices(this IServiceCollection services)
    {
        services.AddBlazoredLocalStorage();
        services.AddScoped<LayoutService>();
        services.AddScoped<DocViewService>();
        services.AddScoped<HubClientService>();
    }

}
