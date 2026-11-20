using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HowIdentity.Pages.SuperAdmin.AppUsers;

using System.Text.Json;
using Common.ResultType;
using Microsoft.AspNetCore.Mvc;
using Services.SuperAdmin;

public class Index : PageModel
{
    private ISuperAdminUserService _superAdminUserService;
    public List<AppUserModel> Users { get; set; } = new();
    [TempData]
    public string ErrorsString { get; set; } = default!;
    public List<ErrorResult> Errors { get; set; } = new();

    public Index(ISuperAdminUserService superAdminUserService)
    {
        _superAdminUserService = superAdminUserService;
    }

    [BindProperty(SupportsGet = true)] public string EmailFilter { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 20;
    [BindProperty] public int UserId { get; set; }
    [BindProperty] public bool IsSuspend { get; set; }
    
    public async Task OnGet()
    {
        if (PageNumber < 1) PageNumber = 1;
        if (PageSize < 1) PageSize = 20;

        if (!string.IsNullOrEmpty(ErrorsString))
        {
            Errors = JsonSerializer.Deserialize<List<ErrorResult>>(ErrorsString);
        }

        var userFromDb = await _superAdminUserService.GetUsers(EmailFilter, PageNumber, PageSize);

        if (userFromDb.IsSuccess)
        {
            Users = userFromDb.Value();
        }
        else
        {
            Errors.AddRange(userFromDb.Errors);
        }
    }

    public async Task<IActionResult> OnPostSuspend()
    {
        var suspendResult = IsSuspend ? 
            await _superAdminUserService.ReSuspendUser(UserId) : 
            await _superAdminUserService.SuspendUser(UserId);

        if (!suspendResult.IsSuccess)
        {
            Errors.AddRange(suspendResult.Errors);
        }

        return RedirectToPage();
    }
    
    public async Task<IActionResult> OnPostDelete()
    {
        var deletedResult = await _superAdminUserService.DeleteUser(UserId);

        if (!deletedResult.IsSuccess)
        {
            Errors.AddRange(deletedResult.Errors);
        }

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
}