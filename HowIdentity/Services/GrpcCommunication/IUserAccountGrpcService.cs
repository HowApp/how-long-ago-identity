namespace HowIdentity.Services.GrpcCommunication;

public interface IUserAccountGrpcService
{
    Task SendRegisterUserRequest(int userId);
}