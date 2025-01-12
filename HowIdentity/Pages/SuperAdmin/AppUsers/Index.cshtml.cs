using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HowIdentity.Pages.SuperAdmin.AppUsers;

using Microsoft.AspNetCore.Mvc;
using Services.SuperAdmin;

public class Index : PageModel
{
    private ISuperAdminUserService _superAdminUserService;
    public List<(int Id, string Email, bool IsSuspended, bool IsDeleted)> Users = new();
    public List<(string KeyError, string MessageError)> Errors = new();

    public Index(ISuperAdminUserService superAdminUserService)
    {
        _superAdminUserService = superAdminUserService;
    }

    [BindProperty(SupportsGet = true)] public string EmailFilter  { get; set; }

    // TODO add pagination
    public async Task OnGet()
    {
        Errors.Clear();
        
        if (_superAdminUserService is not null)
        {
            var userFromDb = await _superAdminUserService.GetUsers();

            if (userFromDb.Success)
            {
                Users = userFromDb.Values;
            }
            else
            {
                Errors.Add(userFromDb.Error);
            }
        }
    }

    public void OnPostSuspend()
    {
        throw new NotImplementedException();
    }
    
    public void OnPostDelete()
    {
        throw new NotImplementedException();
    }
}