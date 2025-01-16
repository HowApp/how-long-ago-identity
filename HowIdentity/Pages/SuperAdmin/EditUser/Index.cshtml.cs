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
    
    [BindProperty(SupportsGet = true)]
    // public (int Id, string Name, bool Apply)[] RoleOptions { get; set; } = [];
    public RoleModel[] RoleOptions { get; set; } = [];
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

        if (!userFromDb.IsSuccess)
        {
            Errors.AddRange(userFromDb.Errors);
            return;
            
        }

        User = userFromDb.Value();
        // RoleOptions = AppConstants.Role.RoleList()
        //     .Select(r => (r.Id, r.Name, User.RoleIds.Contains(r.Id)))
        //     .ToArray();
        RoleOptions = AppConstants.Role.RoleList()
            .Select(r => new RoleModel
            {
                Id = r.Id,
                Name = r.Name,
                Apply = User.RoleIds.Contains(r.Id)
            })
            .ToArray();
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        foreach (var role in RoleOptions)
        {
            Console.WriteLine($"{role.Id} - {role.Name}");
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
}