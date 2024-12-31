namespace HowIdentity;

using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource()
            {
                Name = "role",
                UserClaims = { JwtClaimTypes.Role}
            }
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("web")
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "how-web-app",
                ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:7560/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:7560/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:7560/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = { 
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile, 
                    "web"
                }
            }
        };
}