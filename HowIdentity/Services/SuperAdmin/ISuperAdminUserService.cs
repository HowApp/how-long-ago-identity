namespace HowIdentity.Services.SuperAdmin;

using Common.ResultType;
using Pages.SuperAdmin.AppUsers;

public interface ISuperAdminUserService
{
    public Task<ResultGeneric<List<AppUserModel>>> GetUsers();
    public Task<ResultGeneric<AppUserModel>> GetUserById(int userId);
    public Task<ResultDefault> SuspendUser(int userId);
    public Task<ResultDefault> ReSuspendUser(int userId);
    public Task<ResultDefault> UpdateUserRoles(int userId, (int RoleId, bool State)[] roles, bool forceSessionRemoving = false);
    public Task<ResultDefault> DeleteUser(int userId);
}