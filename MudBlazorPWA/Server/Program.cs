using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.Extensions.FileProviders;
using MudBlazorPWA.Server.Extensions;
using MudBlazorPWA.Server.Hubs;
using MudBlazorPWA.Server.Startup;
using MudBlazorPWA.Shared.Data;
using MudBlazorPWA.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

builder.Services.AddHostServices(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
	app.UseWebAssemblyDebugging();
	using var scope = app.Services.CreateScope();
	var dbInit = scope.ServiceProvider.GetRequiredService<DataContextInitializer>();
	await dbInit.InitialiseAsync();
	await dbInit.SeedDataAsync(removeRecords: false, jsonFilePath: AppConfig.JsonDataSeedFile);
}
else {
	app.UseResponseCompression();
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseCors(AppConfig.CorsPolicy);
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseFileServer(
new FileServerOptions {
	EnableDirectoryBrowsing = true,
	RequestPath = "/files",
	RedirectToAppendTrailingSlash = false,
	DirectoryBrowserOptions = {
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
app.MapHub<EmployeeHub>("/employeeHub");
app.MapFallbackToFile("index.html");

app.Run();
