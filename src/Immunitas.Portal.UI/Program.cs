using Flurl.Http;
using Immunitas.Portal.UI;
using Immunitas.Portal.UI.Auth;
using Immunitas.Portal.UI.Extensions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddMudPopoverService();
builder.Services.AddApiServices();

builder.Services.AddScoped<AuthHandler>();

builder.Services.AddHttpClient("local", client => {
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
})
.AddHttpMessageHandler<AuthHandler>();

builder.Services.AddHttpClient("AuthAPI", client => {
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

builder.Services.AddSingleton<IFlurlClient>(sp => {
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var client = new FlurlClient(factory.CreateClient("local"));
    return client.AllowHttpStatus("401");
});

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());


await builder.Build().RunAsync();
