// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

namespace HowIdentity.Models;

using Microsoft.AspNetCore.Identity;

public class HowUser : IdentityUser<int>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public virtual ICollection<HowUserRole> UserRoles {get; set;}
}