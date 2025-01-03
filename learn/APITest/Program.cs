using APITest;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

Console.Title = "Resource based API";
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAuthentication("token")
    .AddJwtBearer("token", options =>
    {
        // base-address of identityserver
        options.Authority = "https://localhost:5001";
        options.Audience = "api-resource.api-test";
        
        options.TokenValidationParameters.ValidateAudience = false;
        
        // it's recommended to check the type header to avoid "JWT confusion" attacks
        options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
        
        // if token does not contain a dot, it is a reference token
        options.ForwardDefaultSelector = Selector.ForwardReferenceToken("introspection");
    })
    .AddOAuth2Introspection("introspection", options =>
    {
        options.Authority = "https://localhost:5001";

        options.ClientId = "client.api-test";
        options.ClientSecret = "secret.api-test";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.MapControllers().RequireAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
