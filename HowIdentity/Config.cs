﻿namespace HowIdentity;

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
            new ApiScope("scope.web", displayName: "Web Client"),
            new ApiScope("scope.api-test", displayName:"Test API")
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("api-resource.api-test", "API test resource")
            {
                Scopes = { "scope.api-test" },
                ApiSecrets = { new Secret("secrets.api-test".Sha256()) }
            }
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "how-web-app",
                RequireClientSecret = false,
                RequirePkce = true,

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:7560/authentication/login-callback" },
                PostLogoutRedirectUris = { "https://localhost:7560/authentication/logout-callback" },

                AllowOfflineAccess = true,
                AllowedScopes = { 
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile
                }
            },
            
            // blazor standalone web app testing
            new Client
            {
                ClientId = "BlazorTest",
                RequireClientSecret = false,
                RequirePkce = true,

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:7015/authentication/login-callback" },
                PostLogoutRedirectUris = { "https://localhost:7015/authentication/logout-callback" },

                AllowOfflineAccess = true,
                AllowedScopes = { 
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "scope.api-test"
                }
            }
        };
}