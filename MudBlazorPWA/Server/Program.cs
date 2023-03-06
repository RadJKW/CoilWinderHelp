using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.Extensions.FileProviders;

using MudBlazor.Services;
using MudBlazorPWA.Shared;
using MudBlazorPWA.Shared.Data;
using MudBlazorPWA.Shared.Extensions;
using MudBlazorPWA.Shared.Hubs;
using MudBlazorPWA.Shared.Models;
using MudBlazorPWA.Shared.Services;
using MudExtensions.Services;

var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddMudServices();
builder.Services.AddMudExtensions();
builder.Services.AddDirectoryBrowser();
builder.Services.AddSignalR();
builder.Services.AddLogging();
builder.Services.AddLogging(loggingBuilder => {
    loggingBuilder.AddConsole();
});

builder.Services.AddCors(options => {
    options.AddPolicy(AppConfig.CorsPolicy,
    configurePolicy: b => b.AllowAnyMethod()
        .AllowAnyHeader()
        .AllowAnyOrigin());
});

builder.Services.AddHostServices(builder.Configuration);
builder.Services.Configure<DirectoryServiceOptions>(options => {

    options.RootDirectoryPath = AppConfig.BasePath;
});

builder.Services.AddScoped<IDirectoryService, DirectoryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContextInitializer>();
    await dbContext.InitialiseAsync();
    var useInMemoryDatabase = AppConfig.UseInMemoryDatabase;
    var runtimeIsWindows = AppConfig.IsWindows;
    switch (useInMemoryDatabase)
    {
        case true when runtimeIsWindows:
            await dbContext.SeedDataAsync(
                removeRecords: false,
                jsonFilePath: @"C:\Users\jwest\source\RiderProjects\CoilWinderHelp\WindingCodes.json");
            break;
        case true when !runtimeIsWindows:
            await dbContext.SeedDataAsync(
                removeRecords: true,
                jsonFilePath: @"/Users/jkw/RiderProjects/CoilWinderHelp/WindingCodes.json");
            break;
        case false:
            await dbContext.SeedDataAsync(removeRecords: false, jsonFilePath: AppConfig.JsonDataSeedFile);
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
