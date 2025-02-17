namespace HowIdentity.Infrastructure.GrpcCommunication.Client;

public interface IUserAccountGrpcClient
{
    Task<GrpcResult> SendRegisterUserRequest(int userId);
    Task<GrpcResult> SendDeleteUserRequest(int userId);
    Task<GrpcResult> SendSuspendUserRequest(int userId, bool state);
}