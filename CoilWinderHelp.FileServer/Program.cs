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

app.UseFileServer(new FileServerOptions()
{
    EnableDirectoryBrowsing = true,
    RedirectToAppendTrailingSlash = true,
    DirectoryBrowserOptions =
    {
        FileProvider = new PhysicalFileProvider(@"/Users/jkw/WindingPractices/")
        {
            UseActivePolling = true,
            UsePollingFileWatcher = true
        },
    },
    
});

app.Run();
