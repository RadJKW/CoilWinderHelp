using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using MudBlazorPWA.Shared;

const string windowsPath = @"B:\CoilWinderTraining-Edit\";
const string macPath = @"/Users/jkw/WindingPractices/";
const string corsPolicy = "AllowAll";
var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDirectoryBrowser();
// add logging + console logging
builder.Services.AddLogging();
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
});

builder.Services.AddCors(options => {
  options.AddPolicy(corsPolicy,
    configurePolicy: b => b.AllowAnyMethod()
      .AllowAnyHeader()
      .AllowAnyOrigin());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseWebAssemblyDebugging();
}
else
{
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
  DirectoryBrowserOptions = {
      // if the operating system is windows, use the windows path
    // if the operating system is mac, use the mac path
    RedirectToAppendTrailingSlash = false,
    RequestPath = "/files",
    FileProvider = new PhysicalFileProvider(OperatingSystem.IsWindows() ? windowsPath : macPath)
    {
        
      UseActivePolling = true,
      UsePollingFileWatcher = true,
    },
    Formatter = new HtmlDirectorySort(HtmlEncoder.Default)
  }
});

app.UseRouting();
app.UseHttpLogging();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();