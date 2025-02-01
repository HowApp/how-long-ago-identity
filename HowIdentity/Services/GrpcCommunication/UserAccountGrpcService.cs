namespace HowIdentity.Services.GrpcCommunication;

public class UserAccountGrpcService : IUserAccountGrpcService
{
    private readonly UserAccount.UserAccountClient _client;
    private readonly ILogger<UserAccountGrpcService> _logger;

    public UserAccountGrpcService(UserAccount.UserAccountClient client, ILogger<UserAccountGrpcService> logger)
    {
        _client = client;
        _logger = logger;
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
    
    private bool ValidateUserId(int userId)
    {
        if (userId < 1)
        {
            _logger.LogError("Invalid User Id: {UserId}", userId);
            return false;
        }
        return true;
    }
    
    private bool ValidateUserId(int[] userIds)
    {
        if (userIds is null)
        {
            _logger.LogError("User Id collection is invalid");
            return false;
        }
        
        if (userIds.Any(id => id < 1))
        {
            _logger.LogError("Invalid User Id in collection");
            return false;
        }
        
        return true;
    }
}