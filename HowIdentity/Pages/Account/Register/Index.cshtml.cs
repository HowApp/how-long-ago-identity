// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

namespace HowIdentity.Pages.Account.Register;

using Common.Constants;
using Pages;
using Microsoft.AspNetCore.Identity;
using Entity;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using HowCommon.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.UserAccount;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    private readonly UserManager<HowUser> _userManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly ILogger<Index> _logger;
    private readonly IUserAccountService _userAccount;

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public Index(
        IIdentityServerInteractionService interaction,
        UserManager<HowUser> userManager,
        ILogger<Index> logger, 
        IUserAccountService userAccount)
    {
        _interaction = interaction;
        _userManager = userManager;
        _logger = logger;
        _userAccount = userAccount;
    }

    public IActionResult OnGet(string returnUrl)
    {
        Input = new InputModel { ReturnUrl = returnUrl };
        return Page();
    }
        
    public async Task<IActionResult> OnPost()
    {
        // check if we are in the context of an authorization request
        var context = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        // the user clicked the "cancel" button
        if (Input.Button != "create")
        {
            if (context != null)
            {
                // if the user cancels, send a result back into IdentityServer as if they 
                // denied the consent (even if this client does not require consent).
                // this will send back an access denied OIDC error response to the client.
                await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                if (context.IsNativeClient())
                {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage(Input.ReturnUrl);
                }

                return Redirect(Input.ReturnUrl ?? "~/");
            }
            else
            {
                // since we don't have a valid context, then we just go back to the home page
                return Redirect("~/");
            }
        }

        try
        {
            var userNameExists = await _userManager.FindByNameAsync(Input.UserName);
            if (userNameExists != null)
            {
                ModelState.AddModelError("Input.Username", "User with this username already exists.");
                return Page();
            }
        
            var userEmailExists = await _userManager.FindByEmailAsync(Input.Email);
            if (userEmailExists != null)
            {
                ModelState.AddModelError("Input.Username", "User with this email already exists.");
                return Page();
            }
        }
        catch (Exception e)
        {
            ModelState.AddModelError("Internal Error", "Something went wrong. Contact the developers for help.");
            _logger.IdentityRegistrationError(new []{ new IdentityError { Code = "User duplicates in database", Description = $"{e.Message}" } });
            return Page();
        }
        
        if (ModelState.IsValid)
        {
            var user = new HowUser
            {
                UserName = Input.UserName,
                Email = Input.Email,
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
                        ConfirmExisting = false
                    }
                ]
            };
            var userCreateResult = await _userManager.CreateAsync(user, Input.Password);

            if (userCreateResult.Succeeded)
            {
                var addRoleResult = await _userManager.AddToRoleAsync(user, IdentityRoleConstant.Role.User.Name);

                if (!addRoleResult.Succeeded)
                {
                    _logger.IdentityRegistrationError(addRoleResult.Errors);
                    return Redirect("~/");
                }
            }
            else
            {
                foreach (var error in userCreateResult.Errors.Select(e => (e.Code, e.Description)))
                {
                    ModelState.AddModelError(error.Code, error.Description);     
                }
                
                _logger.IdentityRegistrationError(userCreateResult.Errors);
                
                return Page();
            }

            // send event ro create record on main api
            await _userAccount.SendRegisterUserRequest(user.Id);

            // issue authentication cookie with subject ID and username
            var isuser = new IdentityServerUser(user.Id.ToString())
            {
                DisplayName = user.UserName
            };

            await HttpContext.SignInAsync(isuser);

            if (context != null)
            {
                if (context.IsNativeClient())
                {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage(Input.ReturnUrl);
                }

                // we can trust Input.ReturnUrl since GetAuthorizationContextAsync returned non-null
                return Redirect(Input.ReturnUrl ?? "~/");
            }

            // request for a local page
            if (Url.IsLocalUrl(Input.ReturnUrl))
            {
                return Redirect(Input.ReturnUrl);
            }
            else if (string.IsNullOrEmpty(Input.ReturnUrl))
            {
                return Redirect("~/");
            }
            else
            {
                // user might have clicked on a malicious link - should be logged
                _logger.ReturnUrlSuspiciousError($"Invalid return URL: {Input.ReturnUrl}");
            }
        }

        return Page();
    }
}
