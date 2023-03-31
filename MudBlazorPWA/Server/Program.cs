using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.Extensions.FileProviders;
using MudBlazorPWA.Server;
using MudBlazorPWA.Server.Extensions;
using MudBlazorPWA.Server.Hubs;
using MudBlazorPWA.Server.Services;
using MudBlazorPWA.Shared.Data;
using MudBlazorPWA.Shared.Interfaces;
using MudBlazorPWA.Shared.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);



builder.Services.AddHostServices(builder.Configuration);
builder.Services.Configure<DirectoryServiceOptions>(options => {

    options.RootDirectoryPath = AppConfig.BasePath;
});

builder.Services.AddScoped<IDirectoryService, DirectoryService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    using IServiceScope scope = app.Services.CreateScope();
    var dbInit = scope.ServiceProvider.GetRequiredService<DataContextInitializer>();
    await dbInit.InitialiseAsync();
    bool useInMemoryDatabase = AppConfig.UseInMemoryDatabase;
    bool runtimeIsWindows = AppConfig.IsWindows;
    switch (useInMemoryDatabase)
    {
        case true when runtimeIsWindows:
            await dbInit.SeedDataAsync(
            removeRecords: false,
                jsonFilePath: @"C:\Users\jwest\source\RiderProjects\CoilWinderHelp\WindingCodes.json");
            break;
        case true when !runtimeIsWindows:
            await dbInit.SeedDataAsync(
                removeRecords: true,
                jsonFilePath: @"/Users/jkw/RiderProjects/CoilWinderHelp/WindingCodes.json");
            break;
        case false:
            await dbInit.SeedDataAsync(removeRecords: false, jsonFilePath: AppConfig.JsonDataSeedFile);
            break;
    }

}
else
{
    app.UseResponseCompression();
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors(AppConfig.CorsPolicy);
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseFileServer(new FileServerOptions
{
    EnableDirectoryBrowsing = true,
    RequestPath = "/files",
    RedirectToAppendTrailingSlash = false,
    DirectoryBrowserOptions =
    {
        Formatter = new HtmlDirectorySort(HtmlEncoder.Default),
        FileProvider = new PhysicalFileProvider(AppConfig.BasePath),
    }
});

app.UseRouting();
app.UseHttpLogging();
app.MapRazorPages();
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");
app.MapHub<DirectoryHub>("/directoryHub");
app.MapFallbackToFile("index.html");

app.Run();
