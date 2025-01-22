// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.IdentityServer;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HowIdentity.Pages.Home;

using Common.MassTransitContracts.Producer;
using MassTransit;

[AllowAnonymous]
public class Index : PageModel
{
    private readonly IPublishEndpoint _publishEndpoint;
    public Index(IPublishEndpoint publishEndpoint, IdentityServerLicense? license = null)
    {
        _publishEndpoint = publishEndpoint;
        License = license;
    }

    public string Version
    {
        get => typeof(Duende.IdentityServer.Hosting.IdentityServerMiddleware).Assembly
                   .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                   ?.InformationalVersion.Split('+').First()
               ?? "unavailable";
    }

    public IdentityServerLicense? License { get; }

    public async Task OnPostSend()
    {
        await _publishEndpoint.Publish<UserRegisterMessage>(
            new 
            {
                UserIds = 2
            });
    }
}