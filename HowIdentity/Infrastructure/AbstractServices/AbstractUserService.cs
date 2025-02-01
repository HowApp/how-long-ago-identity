namespace HowIdentity.Infrastructure.AbstractServices;

public abstract class AbstractUserService
{
    protected readonly ILogger _logger;

    protected AbstractUserService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    protected bool ValidateUserId(int userId)
    {
        if (userId < 1)
        {
            _logger.LogError("Invalid User Id: {UserId}", userId);
            return false;
        }
        return true;
    }
    
    protected bool ValidateUserId(int[] userIds)
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