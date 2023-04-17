using Blazored.LocalStorage;
using MudBlazor.Services;
using MudBlazorPWA.Client.Services;
using MudExtensions.Services;

namespace MudBlazorPWA.Client;
public static class ConfigureServices
{
    public static void AddClientServices(this IServiceCollection services)
    {
        services.AddMudServices();
        services.AddMudExtensions();
        services.AddBlazoredLocalStorage();
        services.AddScoped<LayoutService>();
        services.AddScoped<DocViewService>();
        services.AddScoped<HubClientService>();
        services.AddScoped<IDirectoryNavigator, DirectoryNavigator>();
        services.AddScoped<AdminEditorState>();
    }

}
