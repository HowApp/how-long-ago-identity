// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

namespace HowIdentity.Pages.Account.Login;

using System.ComponentModel.DataAnnotations;

public class InputModel
{
    [Required] public string UserName { get; set; }
    [Required] public string Password { get; set; }
    public bool RememberLogin { get; set; }
    public string ReturnUrl { get; set; }
    public string Button { get; set; }
}