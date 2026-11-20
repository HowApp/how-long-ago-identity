namespace HowIdentity.Services;

using System.Security.Claims;
using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Entity;

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
        var id = (ClaimsIdentity)principal.Identity ?? new ClaimsIdentity();

        var roles = user.UserRoles is not null
            ? user.UserRoles
                .Select(r => r.Role?.Name)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct()
            : (await UserManager.GetRolesAsync(user)).Distinct();

        foreach (var role in roles)
        {
            if (!id.HasClaim(JwtClaimTypes.Role, role) && !id.HasClaim(ClaimTypes.Role, role))
            {
                id.AddClaim(new Claim(JwtClaimTypes.Role, role));
            }
        }
        context.AddRequestedClaims(principal.Claims);
    }
}