namespace HowIdentity.Infrastructure.Processing.Consumer;

using Dapper;
using Data;
using HowCommon.Enums;
using HowCommon.MassTransitContract;
using MassTransit;

public class UserRegiserBulkResponseConsumer : IConsumer<UserRegiserBulkResponseMessage>
{
    private readonly ILogger<UserRegiserBulkResponseConsumer> _logger;
    private readonly DapperContext _dbContext;

    public UserRegiserBulkResponseConsumer(ILogger<UserRegiserBulkResponseConsumer> logger, DapperContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<UserRegiserBulkResponseMessage> context)
    {
        _logger.LogInformation("Content Received Bulk User ID response");

        if (context.Message.UserIds.Length == 0)
        {
            _logger.LogError("Received User Register Bulk Message without User IDs");
            return;
        }

        var command = $@"
UPDATE user_microservices 
SET confirm_existing = 'TRUE'
WHERE user_id = ANY (@UserIds) AND micro_service = {(int)MicroServicesEnum.MainApi};
";
        try
        {
            await using var connection = _dbContext.InitConnection();

            await connection.ExecuteAsync(command, new
            {
                UserIds = context.Message.UserIds,
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }
}