using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;
using MudBlazorPWA.Shared;
using MudBlazorPWA.Shared.Data;
using MudBlazorPWA.Shared.Extensions;
using MudBlazorPWA.Shared.Hubs;
using MudBlazorPWA.Shared.Services;


const string windowsPath = @"B:\CoilWinderTraining-Edit\";
const string macPath = @"/Users/jkw/WindingPractices/";
const string corsPolicy = "AllowAll";
var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddMudServices();
builder.Services.AddMudExtensions();
builder.Services.AddDirectoryBrowser();
builder.Services.AddSignalR();
// add logging + console logging
builder.Services.AddLogging();
builder.Services.AddLogging(loggingBuilder => {
    loggingBuilder.AddConsole();
});

builder.Services.AddCors(options => {
    options.AddPolicy(corsPolicy,
    configurePolicy: b => b.AllowAnyMethod()
        .AllowAnyHeader()
        .AllowAnyOrigin());
});

builder.Services.AddHostServices(builder.Configuration);
builder.Services.Configure<DirectoryServiceOptions>(options => {

    options.RootDirectoryPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? windowsPath : macPath;
    // options.WindingCodesJsonPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\Users\jwest\source\RiderProjects\CoilWinderHelp\WindingCodes.json" : @"/Users/jkw/RiderProjects/CoilWinderHelp/WindingCodes.json";
});
builder.Services.AddScoped<IDirectoryService, DirectoryService>();

/*builder.Services.AddResponseCompression(opts => {
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
    new[]
    {
        "application/octet-stream"
    });
});*/
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContextInitializer>();
    await dbContext.InitialiseAsync();
    // if appsettings.json has UseInMemoryDatabase set to true, then the database will be seeded with the json file

    var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");
    var runtimeIsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    // switch statement that has a case for useInMemoryDatabase = true and false. THe true case will also have a when clause that checks if the runtime is windows or mac
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
        case false when runtimeIsWindows:
            await dbContext.SeedDataAsync();
            break;
        case false when !runtimeIsWindows:
            await dbContext.SeedDataAsync(
                removeRecords: false,
                jsonFilePath: @"/Users/jkw/RiderProjects/CoilWinderHelp/WindingCodes.json");
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

app.UseCors(corsPolicy);
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
        FileProvider = new PhysicalFileProvider(OperatingSystem.IsWindows() ? windowsPath : macPath)
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
