using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.Authority = "https://localhost:5001";
    options.TokenValidationParameters.ValidateAudience = false;
});

builder.Services.AddAuthorization();

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/identity", (ClaimsPrincipal user) =>
{
   user.Claims.Select(c => new { c.Type, c.Value });
})
.RequireAuthorization();

app.Run();
