using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System;
using ConfigManager.Client;
using Microsoft.AspNetCore.Components.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8081") };

Console.WriteLine($"API Base Address: {httpClient.BaseAddress}");
builder.Services.AddScoped(sp => httpClient);

await builder.Build().RunAsync();



/*

<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <PropertyGroup>
<BlazorBuildMode>WebAssembly</BlazorBuildMode>
<OutputPath>bin\$(Configuration)\net8.0\publish</OutputPath>
    <CopyOutputSymbolsToPublishDirectory>true</CopyOutputSymbolsToPublishDirectory>
    <CopyOutputSymbolsToOutputDirectory>true</CopyOutputSymbolsToOutputDirectory>
</PropertyGroup>

<ItemGroup>
    <ProjectReference Include="..\ConfigManager.Domain\ConfigManager.Domain.csproj" />
    <ProjectReference Include="..\ConfigManager.Application\ConfigManager.Application.csproj" />
    <ProjectReference Include="..\ConfigManager.Infrastructure\ConfigManager.Infrastructure.csproj" />
</ItemGroup>

</Project>



using ConfigManager.Client.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register HttpClient before Build()
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:8081/") });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();*/
