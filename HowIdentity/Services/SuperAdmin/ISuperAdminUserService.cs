namespace HowIdentity.Services.SuperAdmin;

using Common.ResultType;
using Pages.SuperAdmin.AppUsers;

public interface ISuperAdminUserService
{
    public Task<ResultGeneric<List<AppUserModel>>> GetUsers();
    public Task<ResultDefault> SuspendUser(int userId);
    public Task<ResultDefault> ReSuspendUser(int userId);
    public Task<ResultDefault> DeleteUser(int userId);
}