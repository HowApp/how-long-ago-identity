﻿namespace HowIdentity;

using Common.Configurations;
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
                Name = "roles",
                DisplayName = "User Roles",
                UserClaims = { JwtClaimTypes.Role },
                Required = true
            }
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("scope.how-api", displayName:"How API"),
            new ApiScope("scope.api-test", displayName:"Test API") // api for testing
        };

    public static IEnumerable<ApiResource> ApiResources(IConfiguration configuration)
    {
        var identityServerConfiguration = new IdentityServerConfiguration();
        configuration.Bind(nameof(IdentityServerConfiguration), identityServerConfiguration);
        
        var resources = new List<ApiResource>()
        {
            // api for testing
            // name of api resource must correspond to ClientId from api
            new ApiResource("resource.api-test", "API test resource")
            {
                Scopes =
                {
                    "scope.api-test",
                },
                UserClaims = { JwtClaimTypes.Role },
                ApiSecrets = { new Secret("secret.api-test".Sha256()) }
            }
        };

        var resourceHowApi =
            identityServerConfiguration.ApiResources.FirstOrDefault(r => r.ClientId == "resource.how-api");

        if (resourceHowApi is not null)
        {
            resources.Add(
                new ApiResource("resource.how-api", "How API resource")
                {
                    Scopes =
                    {
                        "scope.how-api",
                    },
                    UserClaims = { JwtClaimTypes.Role },
                    ApiSecrets = { new Secret(resourceHowApi.Secret.Sha256()) }
                });
        }

        return resources;
    }

    public static IEnumerable<Client> Clients(IConfiguration configuration)
    {
        var identityServerConfiguration = new IdentityServerConfiguration();
        configuration.Bind(nameof(IdentityServerConfiguration), identityServerConfiguration);
        
        var clients = new List<Client>()
        {
            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "how-web-client",
                ClientName = "How Client blazor wasm",
                RequireClientSecret = false,
                RequirePkce = true,

                AccessTokenType = AccessTokenType.Reference,
                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:7560/authentication/login-callback" },
                PostLogoutRedirectUris = { "https://localhost:7560/authentication/logout-callback" },
                AllowedCorsOrigins = { "https://localhost:7560" },

                AllowOfflineAccess = true,
                AllowedScopes = { 
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "scope.how-api",
                    "roles"
                },
                
                AlwaysIncludeUserClaimsInIdToken = true
            },
            
            // blazor standalone web app testing
            new Client
            {
                ClientId = "BlazorTest",
                ClientName = "Test Client",
                RequireClientSecret = false,
                RequirePkce = true,

                AccessTokenType = AccessTokenType.Reference,
                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:7015/authentication/login-callback" },
                PostLogoutRedirectUris = { "https://localhost:7015/authentication/logout-callback" },
                AllowedCorsOrigins = { "https://localhost:7015" },

                AllowOfflineAccess = true,
                AllowedScopes = { 
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "scope.api-test",
                    "roles"
                },

                AlwaysIncludeUserClaimsInIdToken = true
            }
        };
        
        // swagger client for api
        var clientHowApiSwagger =
            identityServerConfiguration.Clients.FirstOrDefault(r => r.ClientId == "how-api-swagger-client");

        if (clientHowApiSwagger is not null)
        {
            clients.Add(
                new Client
                {
                    ClientId = "how-api-swagger-client",
                    ClientName = "How API Swagger Client",
                    ClientSecrets =
                    {
                        new Secret(clientHowApiSwagger.Secret.Sha256())
                    },
                    RequireClientSecret = true,
                    RequirePkce = true,
        
                    AccessTokenType = AccessTokenType.Jwt,
                    AllowedGrantTypes = GrantTypes.Code,
        
                    RedirectUris = { "https://localhost:7060/swagger/oauth2-redirect.html" },
                    AllowedCorsOrigins = { "https://localhost:7060" },
        
                    AllowOfflineAccess = true,
                    AllowedScopes = { 
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "scope.how-api",
                        "roles"
                    },
                
                    AlwaysIncludeUserClaimsInIdToken = true
                });
        }
        
        return clients;
    }
}