using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using A2AStudio;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddLogging();

builder.Services.AddScoped<IA2AService, A2AService>();

await builder.Build().RunAsync();
