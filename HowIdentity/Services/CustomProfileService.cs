namespace HowIdentity.Services;

using System.Security.Claims;
using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Models;
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
        var id = (ClaimsIdentity)principal.Identity;

        // if (!string.IsNullOrEmpty(user.FavoriteColor))
        // {
        //     id.AddClaim(new Claim("favorite_color", user.FavoriteColor));
        // }
        
        context.AddRequestedClaims(principal.Claims);
    }
}