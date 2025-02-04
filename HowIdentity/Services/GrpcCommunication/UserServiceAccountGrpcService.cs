namespace HowIdentity.Services.GrpcCommunication;

using HowCommon.Infrastructure.Helpers;

public class UserServiceAccountGrpcService : IUserAccountGrpcService
{
    private readonly ILogger<UserServiceAccountGrpcService> _logger;
    private readonly UserAccount.UserAccountClient _client;
    private readonly UserIdHelper _helper;

    public UserServiceAccountGrpcService(
        UserAccount.UserAccountClient client,
        ILogger<UserServiceAccountGrpcService> logger)
    {
        _client = client;
        _logger = logger;
        _helper = new UserIdHelper((sender, message) => logger.LogError(message));
    }

    public async Task SendRegisterUserRequest(int userId)
    {
        if (!_helper.ValidateUserId(userId))
        {
            return;
        }

        var replay = await _client.UserRegisterAsync(new RegisterUserRequest
        {
            UserId = userId
        });

        if (!replay.Success)
        {
            _logger.LogError($"Failed to register user: {userId}");
            // TODO: schedule job to send later
        }
    }

    public async Task SendDeleteUserRequest(int userId)
    {
        if (!_helper.ValidateUserId(userId))
        {
            return;
        }
        
        var replay = await _client.UserDeleteAsync(new DeleteUserRequest
        {
            UserId = userId
        });
        
        if (!replay.Success)
        {
            _logger.LogError($"Failed to delete user: {userId}");
            // TODO: schedule job to send later
        }
    }

    public async Task SendSuspendUserRequest(int userId, bool state)
    {
        if (!_helper.ValidateUserId(userId))
        {
            return;
        }
        
        var replay = await _client.UserSuspendAsync(new SuspendUser()
        {
            UserId = userId,
            State = state
        });
        
        if (!replay.Success)
        {
            _logger.LogError($"Failed to suspend user: {userId}");
            // TODO: schedule job to send later
        }
    }
}