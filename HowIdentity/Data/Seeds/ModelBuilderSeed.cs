namespace HowIdentity.Data.Seeds;

using Common.Constants;
using Models;
using Microsoft.EntityFrameworkCore;

public static class ModelBuilderSeed
{
    public static ModelBuilder SeedRoles(this ModelBuilder modelBuilder)
    {
        var roles = new HowRole[]
        {
            new ()
            {
                Id = AppConstants.Role.User.Id,
                Name = AppConstants.Role.User.Name,
                NormalizedName = AppConstants.Role.User.Name.ToUpper(),
                ConcurrencyStamp = "88d484e2-ee7a-49b8-95e6-e34qw5rqb625"
            },
            new ()
            {
                Id = AppConstants.Role.Admin.Id,
                Name = AppConstants.Role.Admin.Name,
                NormalizedName = AppConstants.Role.Admin.Name.ToUpper(),
                ConcurrencyStamp = "8b6258e2-ee7a-49b8-95e6-e34qw5rqd484"
            },
            new ()
            {
                Id = AppConstants.Role.SuperAdmin.Id,
                Name = AppConstants.Role.SuperAdmin.Name,
                NormalizedName = AppConstants.Role.SuperAdmin.Name.ToUpper(),
                ConcurrencyStamp = "8z2358e2-ee7a-49b8-95e6-e34qw5rqd484"
            }
        };

        modelBuilder.Entity<HowRole>().HasData(roles);

        modelBuilder.Entity<HowRole>().Property(r => r.Id)
            .HasIdentityOptions(startValue: roles.Length + 1);

        return modelBuilder;
    }
}