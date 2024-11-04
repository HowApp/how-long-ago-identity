using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.Authority = "https://localhost:5001";
    options.TokenValidationParameters.ValidateAudience = false;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "api1");
    });
});

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/identity", (ClaimsPrincipal user) =>
{
   return user.Claims.Select(c => new { c.Type, c.Value });
})
.RequireAuthorization();

app.MapGet("/identity/with-policy", (ClaimsPrincipal user) =>
    {
        return user.Claims.Select(c => new { c.Type, c.Value });
    })
    .RequireAuthorization("ApiScope");

app.Run();
