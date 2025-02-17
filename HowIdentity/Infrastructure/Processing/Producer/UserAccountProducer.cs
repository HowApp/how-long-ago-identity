namespace HowIdentity.Infrastructure.Processing.Producer;

using HowCommon.Infrastructure.Helpers;
using HowCommon.MassTransitContract;
using MassTransit;

public class UserAccountProducer
{
    private readonly ILogger<UserAccountProducer> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly UserIdHelper _helper;

    public UserAccountProducer(
        IPublishEndpoint publishEndpoint,
        ILogger<UserAccountProducer> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;

        _helper = new UserIdHelper((sender, message) => logger.LogError(message));
    }

    public async Task PublishUserRegistrationMessage(int userId)
    {
        if (!_helper.ValidateUserId(userId))
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
        if (!_helper.ValidateUserId(userIds))
        {
            return;
        }

        if (userIds.Length == 0)
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
        if (!_helper.ValidateUserId(userId))
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
        if (!_helper.ValidateUserId(userId))
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
}