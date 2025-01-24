namespace HowIdentity.Data.Seeds;

using Common.Constants;
using Entity;
using Microsoft.EntityFrameworkCore;

public static class ModelBuilderSeed
{
    public static ModelBuilder SeedRoles(this ModelBuilder modelBuilder)
    {
        var roles = new HowRole[]
        {
            new ()
            {
                Id = IdentityRoleConstant.Role.User.Id,
                Name = IdentityRoleConstant.Role.User.Name,
                NormalizedName = IdentityRoleConstant.Role.User.Name.ToUpper(),
                ConcurrencyStamp = "88d484e2-ee7a-49b8-95e6-e34qw5rqb625"
            },
            new ()
            {
                Id = IdentityRoleConstant.Role.Admin.Id,
                Name = IdentityRoleConstant.Role.Admin.Name,
                NormalizedName = IdentityRoleConstant.Role.Admin.Name.ToUpper(),
                ConcurrencyStamp = "8b6258e2-ee7a-49b8-95e6-e34qw5rqd484"
            },
            new ()
            {
                Id = IdentityRoleConstant.Role.SuperAdmin.Id,
                Name = IdentityRoleConstant.Role.SuperAdmin.Name,
                NormalizedName = IdentityRoleConstant.Role.SuperAdmin.Name.ToUpper(),
                ConcurrencyStamp = "8z2358e2-ee7a-49b8-95e6-e34qw5rqd484"
            }
        };

        modelBuilder.Entity<HowRole>().HasData(roles);

        modelBuilder.Entity<HowRole>().Property(r => r.Id)
            .HasIdentityOptions(startValue: roles.Length + 1);

        return modelBuilder;
    }
}