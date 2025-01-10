namespace HowIdentity.Data;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Entity;
using Extensions;

public class ApplicationDbContext : IdentityDbContext<
    HowUser, 
    HowRole, 
    int, 
    HowUserClaim, 
    HowUserRole, 
    HowUserLogin, 
    HowRoleClaim, 
    HowUserToken>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
        
        builder.SetIdentityName();
        builder.SetIdentityRule();
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        builder.SetOnDeleteRule();
        builder.UseSnakeCaseNamingConvention();
    }
}