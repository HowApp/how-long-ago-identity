﻿// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

namespace HowIdentity.Entity;

using Microsoft.AspNetCore.Identity;

public class HowUser : IdentityUser<int>
{
    public bool IsDeleted { get; set; }
    public bool IsSuspended{ get; set; }
    public virtual ICollection<HowUserRole> UserRoles {get; set;}
}