using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TravelRequestManagement.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<ITravelApi, TravelApi>();
builder.Services.AddScoped<AuthState>();
builder.Services.AddScoped<DashboardNotifier>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

await builder.Build().RunAsync();
