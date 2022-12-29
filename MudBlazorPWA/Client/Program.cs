using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using MudBlazorPWA.Client;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddHttpClient("PwaServer", client => 
        client
        .BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services
    .AddHttpClient("LocalHostServer", client => 
        client
        .BaseAddress = new Uri("http://localhost:3000"));
builder.Services
    .AddHttpClient("MacServer", client => 
        client.BaseAddress = new Uri("http://192.168.0.15:3000"));
builder.Services
    .AddHttpClient("ApiFileServer", client => 
        client
        .BaseAddress = new Uri($"http://localhost:5010"));
builder.Services
    .AddHttpClient("ApiVideoServer", client => 
        client
        .BaseAddress = new Uri($"https://localhost:7188/video/"));


builder.Services.AddMudServices();


await builder.Build().RunAsync();
