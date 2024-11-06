﻿using Duende.IdentityServer.Models;

namespace IdentityServer;

using Duende.IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        { 
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope(name: "api1", displayName: "My API")
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // machine to machine client
            new Client
            {
                ClientId = "client",
                
                // machine to machine
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                
                // secret for authorization
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                
                // define list of scopes that this client cat access
                AllowedScopes = { "api1" }
            },
            new Client
            {
                ClientId = "web",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = { "https://localhost:5002/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                }
            }
        };
}