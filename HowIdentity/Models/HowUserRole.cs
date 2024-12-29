namespace HowIdentity.Models;

using Microsoft.AspNetCore.Identity;

public class HowUserRole : IdentityUserRole<int>
{
    public virtual HowUser User { get; set; }
    public virtual HowRole Role { get; set; }
}