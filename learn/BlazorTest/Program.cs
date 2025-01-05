using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorTest;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("api-test-client.authorized", client => 
    client.BaseAddress = new Uri("https://localhost:7088"))
    .AddHttpMessageHandler(httpBuilder =>
    {
        var handler = httpBuilder.GetService<AuthorizationMessageHandler>()!
            .ConfigureHandler(
                authorizedUrls: new[] { "https://localhost:7088" },
                scopes: new[] { "scope.api-test" });
        
        return handler;
    });

builder.Services.AddHttpClient("api-test-client.unauthorized", client =>
{
    client.BaseAddress = new Uri("https://localhost:7088");
});

builder.Services.AddScoped(sp =>  sp.GetService<IHttpClientFactory>()!.CreateClient("api-test-client.authorized"));
builder.Services.AddScoped(sp =>  sp.GetService<IHttpClientFactory>()!.CreateClient("api-test-client.unauthorized"));
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddOidcAuthentication(options =>
{
    // Configure your authentication provider options here.
    // For more information, see https://aka.ms/blazor-standalone-auth
    builder.Configuration.Bind("oidc", options.ProviderOptions);
    options.UserOptions.RoleClaim = "role";
});

await builder.Build().RunAsync();
