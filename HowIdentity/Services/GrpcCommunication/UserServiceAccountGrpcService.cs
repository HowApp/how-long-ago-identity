namespace HowIdentity.Services.GrpcCommunication;

using Infrastructure.AbstractServices;

public class UserServiceAccountGrpcService : AbstractUserService, IUserAccountGrpcService
{
    private readonly UserAccount.UserAccountClient _client;

    public UserServiceAccountGrpcService(UserAccount.UserAccountClient client, ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        _client = client;
    }

    public async Task SendRegisterUserRequest(int userId)
    {
        if (!ValidateUserId(userId))
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
        if (!ValidateUserId(userId))
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
        if (!ValidateUserId(userId))
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