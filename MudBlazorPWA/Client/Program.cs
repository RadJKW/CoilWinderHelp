using System.Collections.Immutable;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MudBlazor.Services;
using MudBlazorPWA.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// builder.Services.AddScoped<IJSRuntime, JSRuntime>();

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services
    .AddHttpClient("PwaServer", client => client
        .BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services
    .AddHttpClient("FileServer", client => client
        .BaseAddress = new Uri("http://localhost:3000"));
builder.Services
    .AddHttpClient("ApiFileServer", client => client
        .BaseAddress = new Uri($"https://localhost:7188/file/"));
builder.Services
    .AddHttpClient("ApiVideoServer", client => client
        .BaseAddress = new Uri($"https://localhost:7188/video/"));


builder.Services.AddMudServices();


await builder.Build().RunAsync();
