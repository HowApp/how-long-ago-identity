namespace HowIdentity.Infrastructure.GrpcCommunication.Client;

using HowCommon.Infrastructure.Helpers;

public class UserAccountGrpcClient : IUserAccountGrpcClient
{
    private readonly ILogger<UserAccountGrpcClient> _logger;
    private readonly UserAccount.UserAccountClient _client;
    private readonly UserIdHelper _helper;

    public UserAccountGrpcClient(
        UserAccount.UserAccountClient client,
        ILogger<UserAccountGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
        _helper = new UserIdHelper((sender, message) => logger.LogError(message));
    }

    public async Task<GrpcResult> SendRegisterUserRequest(int userId)
    {
        if (!_helper.ValidateUserId(userId))
        {
            return await Task.FromResult(GrpcResult.ValidationError);
        }

        var replay = await _client.UserRegisterAsync(new RegisterUserRequest
        {
            UserId = userId
        });

        if (!replay.Success)
        {
            _logger.LogError($"Failed to register user: {userId}");
        }

        return replay.Success ? GrpcResult.Success : GrpcResult.Error;
    }

    public async Task<GrpcResult> SendDeleteUserRequest(int userId)
    {
        if (!_helper.ValidateUserId(userId))
        {
            return await Task.FromResult(GrpcResult.ValidationError);
        }
        
        var replay = await _client.UserDeleteAsync(new DeleteUserRequest
        {
            UserId = userId
        });
        
        if (!replay.Success)
        {
            _logger.LogError($"Failed to delete user: {userId}");
        }
        
        return replay.Success ? GrpcResult.Success : GrpcResult.Error;
    }

    public async Task<GrpcResult> SendSuspendUserRequest(int userId, bool state)
    {
        if (!_helper.ValidateUserId(userId))
        {
            return await Task.FromResult(GrpcResult.ValidationError);
        }
        
        var replay = await _client.UserSuspendAsync(new SuspendUser()
        {
            UserId = userId,
            State = state
        });
        
        if (!replay.Success)
        {
            _logger.LogError($"Failed to suspend user: {userId}");
        }
        
        return replay.Success ? GrpcResult.Success : GrpcResult.Error;
    }
}