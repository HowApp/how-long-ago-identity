namespace HowIdentity.Infrastructure.Processing.Producer;

using HowCommon.MassTransitContract;
using MassTransit;

public class UserAccountProducer
{
    private readonly ILogger<UserAccountProducer> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public UserAccountProducer(IPublishEndpoint publishEndpoint, ILogger<UserAccountProducer> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishUserRegistrationMessage(int userId)
    {
        if (!ValidateUserId(userId))
        {
            return;
        }

        await PublishMessageAsync(new UserRegisterMessage
        {
            UserId = userId
        });
    }
    
    public async Task PublishUserBulkRegistrationMessage(int[] userIds)
    {
        if (!ValidateUserId(userIds))
        {
            return;
        }
        
        await PublishMessageAsync(new UserRegiserBulkMessage
        {
            UserIds = userIds
        });
    }
    
    public async Task PublishUserDeletedMessage(int userId)
    {
        if (!ValidateUserId(userId))
        {
            return;
        }

        await PublishMessageAsync(new UserDeletedMessage
        {
            UserId = userId
        });
    }
    
    public async Task PublishUserSuspendMessage(int userId, bool state)
    {
        if (!ValidateUserId(userId))
        {
            return;
        }

        await PublishMessageAsync(new UserSuspendedStateMessage
        {
            UserId = userId,
            IsSuspended = state
        });
    }
    
    private async Task PublishMessageAsync<T>(T message) where T : class
    {
        if (message == null)
        {
            _logger.LogError("Attempted to publish a null message of type {MessageType}", typeof(T).Name);
            return;
        }

        try
        {
            await _publishEndpoint.Publish(message);
            _logger.LogInformation("Published message of type {MessageType}", typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message of type {MessageType}", typeof(T).Name);
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