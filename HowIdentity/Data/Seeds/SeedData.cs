namespace HowIdentity.Data.Seeds;

using Common.Constants;
using Serilog;
using System.Security.Claims;
using Common.Configurations;
using IdentityModel;
using Data;
using Entity;
using HowCommon.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<HowUser>>();
            var alice = userMgr.FindByEmailAsync("AliceSmith@email.com").Result;
            if (alice == null)
            {
                alice = new HowUser
                {
                    UserName = "alice",
                    Email = "AliceSmith@email.com",
                    EmailConfirmed = true,
                    UserMicroservices =
                    [
                        new UserMicroservices
                        {
                            MicroService = MicroServicesEnum.IdentityServer,
                            ConfirmExisting = true
                        },
                        new UserMicroservices
                        {
                            MicroService = MicroServicesEnum.MainApi,
                            ConfirmExisting = true
                        }
                    ]
                };
                var result = userMgr.CreateAsync(alice, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(alice, new Claim[]
                {
                    new Claim(JwtClaimTypes.Name, "Alice Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Alice"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                
                var addRole = userMgr.AddToRoleAsync(alice, IdentityRoleConstant.Role.User.Name).Result;
                if (!addRole.Succeeded)
                {
                    throw new Exception(addRole.Errors.First().Description);
                }
                
                Log.Debug("alice created");
            }
            else
            {
                Log.Debug("alice already exists");
            }

            var bob = userMgr.FindByEmailAsync("BobSmith@email.com").Result;
            if (bob == null)
            {
                bob = new HowUser
                {
                    UserName = "bob",
                    Email = "BobSmith@email.com",
                    EmailConfirmed = true,
                    UserMicroservices = [
                        new UserMicroservices
                        {
                            MicroService = MicroServicesEnum.IdentityServer,
                            ConfirmExisting = true
                        },
                        new UserMicroservices
                        {
                            MicroService = MicroServicesEnum.MainApi,
                            ConfirmExisting = true
                        }
                    ]
                };
                var result = userMgr.CreateAsync(bob, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(bob, new Claim[]
                {
                    new Claim(JwtClaimTypes.Name, "Bob Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Bob"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                    new Claim("location", "somewhere")
                }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                
                var addRole = userMgr.AddToRoleAsync(bob, IdentityRoleConstant.Role.User.Name).Result;
                if (!addRole.Succeeded)
                {
                    throw new Exception(addRole.Errors.First().Description);
                }

                Log.Debug("bob created");
            }
            else
            {
                Log.Debug("bob already exists");
            }
        }
    }
    
    public static void EnsureSeedAdmin(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();

            var userManager = scope.ServiceProvider.GetService<UserManager<HowUser>>();
            var adminOptions = scope.ServiceProvider.GetService<IOptions<AdminCredentials>>();
            
            var adminCredentials = adminOptions.Value;
            
            var user = userManager!.FindByEmailAsync(adminCredentials.Email).Result;

            if (user is null)
            {
                user = new HowUser
                {
                    UserName = adminCredentials.Name,
                    Email = adminCredentials.Email,
                    EmailConfirmed = true,
                    UserMicroservices = [
                        new UserMicroservices
                        {
                            MicroService = MicroServicesEnum.IdentityServer,
                            ConfirmExisting = true
                        },
                        new UserMicroservices
                        {
                            MicroService = MicroServicesEnum.MainApi,
                            ConfirmExisting = true
                        }
                    ],
                    UserRoles = new List<HowUserRole>
                    {
                        new ()
                        {
                            RoleId = IdentityRoleConstant.Role.SuperAdmin.Id
                        }
                    }
                };

                var result = userManager!.CreateAsync(user, adminCredentials.Password).Result;
            
                if (!result.Succeeded)
                {
                    throw new Exception("Super Admin not created");
                }
            }
        }
    }
}