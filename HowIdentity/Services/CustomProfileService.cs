namespace HowIdentity.Services;

using System.Security.Claims;
using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Models;

public class CustomProfileService : ProfileService<HowUser>
{
    public CustomProfileService(
        UserManager<HowUser> userManager,
        IUserClaimsPrincipalFactory<HowUser> claimsFactory) 
        : base(userManager, claimsFactory)
    {
    }

    protected override async Task GetProfileDataAsync(ProfileDataRequestContext context, HowUser user)
    {
        var principal = await GetUserClaimsAsync(user);
        var id = (ClaimsIdentity)principal.Identity?? new ClaimsIdentity();

        if (user.UserRoles is not null)
        {
            var userRoles = user.UserRoles
                .Select(r => r.Role.Name)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct();

            foreach (var role in userRoles)
            {
                id.AddClaim(new Claim(JwtClaimTypes.Role, role));
            }
        }
        context.AddRequestedClaims(principal.Claims);
    }
}