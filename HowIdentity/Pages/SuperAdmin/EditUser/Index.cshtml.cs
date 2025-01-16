using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HowIdentity.Pages.SuperAdmin.EditUser;

using System.Text.Json;
using AppUsers;
using Common.Constants;
using Common.ResultType;
using Microsoft.AspNetCore.Mvc;
using Services.SuperAdmin;

public class Index : PageModel
{
    private ISuperAdminUserService _superAdminUserService;
    
    public RoleModel[] RoleOptions { get; set; } = [];
    [BindProperty]
    public RoleUpdateModel[] RoleOptionsUpdate { get; set; } = [];
    [BindProperty]
    public int UserId { get; set; }
    
    public AppUserModel User { get; set; } = default!;
    public string ErrorsString { get; set; } = default!;
    public List<ErrorResult> Errors { get; set; } = new();
    
    
    public Index(ISuperAdminUserService superAdminUserService)
    {
        _superAdminUserService = superAdminUserService;
    }
    
    public async Task OnGet(int id)
    {
        if (!string.IsNullOrEmpty(ErrorsString))
        {
            Errors = JsonSerializer.Deserialize<List<ErrorResult>>(ErrorsString);
        }
        
        var userFromDb = await _superAdminUserService.GetUserById(id);

        if (userFromDb.IsSuccess)
        {
            User = userFromDb.Value();
            RoleOptions = AppConstants.Role.RoleList()
                .Select(r => new RoleModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Apply = User.RoleIds.Contains(r.Id)
                })
                .ToArray();

            RoleOptionsUpdate = new RoleUpdateModel[RoleOptions.Length];
        }
        else
        {
            Errors.AddRange(userFromDb.Errors);
        }
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        foreach (var role in RoleOptionsUpdate)
        {
            Console.WriteLine($"{role.Id}");
        }
        
        await Task.Delay(500);
        return RedirectToPage();
    }
    
    private void TempDataStore()
    {
        ErrorsString = JsonSerializer.Serialize(Errors);
    }

    public override RedirectToPageResult RedirectToPage()
    {
        TempDataStore();
        return base.RedirectToPage();
    }

    public class RoleModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Apply { get; set; }
    }
    public class RoleUpdateModel
    {
        public int Id { get; set; }
        public bool Apply { get; set; }
    }
}