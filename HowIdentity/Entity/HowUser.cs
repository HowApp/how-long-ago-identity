namespace HowIdentity.Entity;

using Microsoft.AspNetCore.Identity;

public class HowUser : IdentityUser<int>
{
    public bool IsDeleted { get; set; }
    public bool IsSuspended{ get; set; }
    public virtual ICollection<HowUserRole> UserRoles {get; set;}
    public ICollection<UserMicroservices> UserMicroservices { get; set; }
}