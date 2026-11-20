namespace HowIdentity.Services.UserAccount;

using Dapper;
using Data;
using HowCommon.Enums;
using Infrastructure.GrpcCommunication;
using Infrastructure.GrpcCommunication.Client;
using Infrastructure.Processing.Producer;

public class UserAccountService : IUserAccountService
{
    private readonly IUserAccountGrpcClient _userAccountGrpcClient;
    private readonly UserAccountProducer _userAccountProducer;
    private readonly DapperContext _dapperContext;

    public UserAccountService(
        IUserAccountGrpcClient userAccountGrpcClient,
        UserAccountProducer userAccountProducer,
        DapperContext dapperContext)
    {
        _userAccountGrpcClient = userAccountGrpcClient;
        _userAccountProducer = userAccountProducer;
        _dapperContext = dapperContext;
    }

    public async Task SendRegisterUserRequest(int userId)
    {
        var grpcResult = await _userAccountGrpcClient.SendRegisterUserRequest(userId);
        switch (grpcResult)
        {
            case GrpcResult.Error:
                await _userAccountProducer.PublishUserRegistrationMessage(userId);
                break;
            case GrpcResult.Success:
                await using (var connection = _dapperContext.InitConnection())
                {
                    await connection.ExecuteAsync(
                        "UPDATE user_microservices SET confirm_existing = TRUE WHERE user_id = @UserId AND micro_service = @MicroService;",
                        new { UserId = userId, MicroService = (int)MicroServicesEnum.MainApi });
                }
                return;
            case GrpcResult.ValidationError:
                return;
            default:
                throw new ArgumentException($"Unknown result type of {nameof(GrpcResult)}: {grpcResult}");
        }
    }

    public async Task SendDeleteUserRequest(int userId)
    {
        var grpcResult = await _userAccountGrpcClient.SendDeleteUserRequest(userId);
        switch (grpcResult)
        {
            case GrpcResult.Error:
                await _userAccountProducer.PublishUserDeletedMessage(userId);
                break;
            case GrpcResult.Success:
            case GrpcResult.ValidationError:
                return;
            default:
                throw new ArgumentException($"Unknown result type of {nameof(GrpcResult)}: {grpcResult}");
        }
    }

    public async Task SendSuspendUserRequest(int userId, bool state)
    {
        var grpcResult = await _userAccountGrpcClient.SendSuspendUserRequest(userId, state);
        switch (grpcResult)
        {
            case GrpcResult.Error:
                await _userAccountProducer.PublishUserSuspendMessage(userId, state);
                break;
            case GrpcResult.Success:
            case GrpcResult.ValidationError:
                return;
            default:
                throw new ArgumentException($"Unknown result type of {nameof(GrpcResult)}: {grpcResult}");
        }
    }
}