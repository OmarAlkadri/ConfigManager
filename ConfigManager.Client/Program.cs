using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System;
using ConfigManager.Client;
using Microsoft.AspNetCore.Components.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
var port = Environment.GetEnvironmentVariable("CONFIG_MANAGER_PORT") ?? "8081";
var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{port}") };

Console.WriteLine($"API Base Address: {httpClient.BaseAddress}");
builder.Services.AddScoped(sp => httpClient);

await builder.Build().RunAsync();