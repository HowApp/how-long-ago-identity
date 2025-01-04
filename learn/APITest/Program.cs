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

builder.Services.AddSerilog();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("API-cors", appBuilder =>
    {
        appBuilder.WithOrigins(new[] {"https://localhost:7088", "http://localhost:5024"})
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add services to the container.
builder.Services.AddAuthentication("token")
    .AddJwtBearer("token", options =>
    {
        // base-address of identityserver
        options.Authority = "https://localhost:5001";
        options.Audience = "api-resource.api-test";
        
        options.TokenValidationParameters.ValidateAudience = true;
        
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
app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("API-cors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireAuthorization();

app.Run();
