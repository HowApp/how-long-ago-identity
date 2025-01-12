using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HowIdentity.Pages.SuperAdmin.AppUsers;

using System.Text.Json;
using Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services.SuperAdmin;

public class Index : PageModel
{
    private ISuperAdminUserService _superAdminUserService;
    public List<(int Id, string Email, bool IsSuspended, bool IsDeleted)> Users { get; set; } = new();
    // public List<(string KeyError, string MessageError)> Errors { get; set; } = new();
    [TempData]
    public string ErrorsString { get; set; } = default!;
    public List<PageErrorModel> Errors { get; set; } = new();

    public Index(ISuperAdminUserService superAdminUserService)
    {
        _superAdminUserService = superAdminUserService;
    }

    [BindProperty(SupportsGet = true)] public string EmailFilter  { get; set; }
    [BindProperty] public int UserId { get; set; }
    [BindProperty] public bool Suspend { get; set; }

    // TODO add pagination
    public async Task OnGet()
    {
        if (!string.IsNullOrEmpty(ErrorsString))
        {
            Errors = JsonSerializer.Deserialize<List<PageErrorModel>>(ErrorsString);
        }

        var userFromDb = await _superAdminUserService.GetUsers();

        if (userFromDb.Success)
        {
            Users = userFromDb.Values;
        }
        else
        {
            Errors.AddError(userFromDb.Error);
        }
    }

    public async Task<IActionResult> OnPostSuspend()
    {
        var suspendResult = Suspend ? 
            await _superAdminUserService.SuspendUser(UserId) : 
            await _superAdminUserService.ReSuspendUser(UserId);
        
        if (!suspendResult.Success)
        {
            Errors.AddError(suspendResult.Error);
        }

        TempDataStore();
        return RedirectToPage();
    }
    
    public async Task<IActionResult> OnPostDelete()
    {
        var deletedResult = await _superAdminUserService.DeleteUser(UserId);
        
        if (!deletedResult.Success)
        {
            Errors.AddError(deletedResult.Error);
        }

        TempDataStore();
        return RedirectToPage();
    }

    private void TempDataStore()
    {
        ErrorsString = JsonSerializer.Serialize(Errors);
    }
}