namespace HowIdentity.Services.UserAccount;

public interface IUserAccountService
{
    Task SendRegisterUserRequest(int userId);
    Task SendDeleteUserRequest(int userId);
    Task SendSuspendUserRequest(int userId, bool state);
}