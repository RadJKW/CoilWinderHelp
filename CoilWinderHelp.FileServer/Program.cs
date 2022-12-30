using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDirectoryBrowser();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
    b => b.AllowAnyMethod()
        .AllowAnyHeader()
        .AllowAnyOrigin());
});


var app = builder.Build();
app.UseCors("AllowAll");

var windowsPath = @"B:\CoilWinderTraining-Edit\";
var macPath = @"/Users/jkw/WindingPractices/";
app.UseFileServer(new FileServerOptions()
{
    
    EnableDirectoryBrowsing = true,
    RedirectToAppendTrailingSlash = true,
    DirectoryBrowserOptions =
    {
        /*FileProvider = new PhysicalFileProvider(@"/Users/jkw/WindingPractices/")
        {
            UseActivePolling = true,
            UsePollingFileWatcher = true
        }*/
        
        // if the operating system is windows, use the windows path
        // if the operating system is mac, use the mac path
        FileProvider = new PhysicalFileProvider(OperatingSystem.IsWindows() ? windowsPath : macPath)
        {
            UseActivePolling = true,
            UsePollingFileWatcher = true
        }
    },
    
});

app.Run();
