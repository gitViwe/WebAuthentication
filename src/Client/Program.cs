using Client;
using Client.Manager;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var baseUri = new Uri(builder.HostEnvironment.BaseAddress);

var apiUri = baseUri.Scheme + Uri.SchemeDelimiter + baseUri.Host + "/:5291";

builder.Services
    .AddScoped<KanBanManager>()
    .AddScoped<WebAuthenticationManager>()
	.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"))
    .AddHttpClient("API", client => client.BaseAddress = new Uri(apiUri));
builder.Services.AddMudServices();

await builder.Build().RunAsync();
