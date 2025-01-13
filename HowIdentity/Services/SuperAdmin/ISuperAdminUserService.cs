namespace HowIdentity.Services.SuperAdmin;

public interface ISuperAdminUserService
{
    public Task<(
            List<(int Id, string Email, string Roles, bool IsSuspended, bool IsDeleted)> Values,
            bool Success,
            (string KeyError, string MessageError) Error)>
        GetUsers();
    public Task<(bool Success, (string KeyError, string MessageError) Error)> SuspendUser(int userId);
    public Task<(bool Success, (string KeyError, string MessageError) Error)> ReSuspendUser(int userId);
    public Task<(bool Success, (string KeyError, string MessageError) Error)> DeleteUser(int userId);
}