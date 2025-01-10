namespace HowIdentity.Entity;

using Microsoft.AspNetCore.Identity;

public class HowRole : IdentityRole<int>
{
    public virtual ICollection<HowUserRole> UserRoles { get; set; }
}