// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

namespace HowIdentity.Pages.Account.Register;

using System.ComponentModel.DataAnnotations;

public class InputModel
{
    [Required]
    public string UserName { get; set; }

    [Required]
    public string Password { get; set; }

    public string Name { get; set; }
    public string Email { get; set; }

    public string ReturnUrl { get; set; }

    public string Button { get; set; }
}