namespace HowIdentity.Services.GrpcCommunication;

public interface IUserAccountGrpcService
{
    Task SendRegisterUserRequest(int userId);
    Task SendDeleteUserRequest(int userId);
    Task SendSuspendUserRequest(int userId, bool state);
}