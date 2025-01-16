namespace HowIdentity.Pages.SuperAdmin.AppUsers;

public class AppUserModel
{
    public int Id {get;set;}
    public string Email {get;set;}
    public string Roles {get;set;}
    public int[] RoleIds {get;set;} = [];
    public bool IsSuspended {get;set;}
    public bool IsDeleted {get;set;}
}