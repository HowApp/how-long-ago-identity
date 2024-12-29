namespace HowIdentity.Models;

using Microsoft.AspNetCore.Identity;

public class HowRole : IdentityRole<int>
{
    public virtual ICollection<HowUserRole> UserRoles { get; set; }
}