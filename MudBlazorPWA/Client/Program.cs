using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazorPWA.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var hostAddress = new Uri(builder.HostEnvironment.BaseAddress);
var webDirectory = new Uri(hostAddress, "WindingPractices");

builder.Services
       .AddHttpClient("PwaServer", configureClient: client =>
	       client
		       .BaseAddress = new(builder.HostEnvironment.BaseAddress));

builder.Services
  .AddHttpClient("ApiFileServer", configureClient: client =>
    client
      .BaseAddress = webDirectory);


builder.Services.AddClientServices();

await builder.Build().RunAsync();
