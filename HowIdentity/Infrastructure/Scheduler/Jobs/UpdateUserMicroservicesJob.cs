namespace HowIdentity.Infrastructure.Scheduler.Jobs;

using Dapper;
using Data;
using Entity;
using HowCommon.Enums;
using HowCommon.Extensions;
using Npgsql;
using Processing.Producer;
using Quartz;

public class UpdateUserMicroservicesJob : IJob
{
    private readonly ILogger<UpdateUserMicroservicesJob> _logger;
    private readonly DapperContext _context;
    private readonly UserServiceAccountProducer _producer;

    public UpdateUserMicroservicesJob(
        ILogger<UpdateUserMicroservicesJob> logger,
        DapperContext context,
        UserServiceAccountProducer producer)
    {
        _logger = logger;
        _context = context;
        _producer = producer;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var microservices = new int[]
        {
            (int)MicroServicesEnum.IdentityServer,
            (int)MicroServicesEnum.MainApi
        };

        try
        {
            using var connection = _context.InitConnection();
            var queryResult = await connection.QueryAsync<(int userId, int microserviceId)>(
                _query,
                new
                {
                    Microservices = microservices
                });

            var userList = queryResult.ToList();

            if (!userList.Any())
            {
                return;
            }

            var usersNeedAddToApi = userList
                .Where(u => u.microserviceId == (int)MicroServicesEnum.MainApi)
                .Select(u => u.userId).ToArray();

            if (usersNeedAddToApi.Any())
            {
                await _producer.PublishUserBulkRegistrationMessage(usersNeedAddToApi);

                await CommandExecute(usersNeedAddToApi, MicroServicesEnum.MainApi, connection);
            }

            var usersNeedAddIdentityApi = userList
                .Where(u => u.microserviceId == (int)MicroServicesEnum.IdentityServer)
                .Select(u => u.userId).ToArray();

            if (usersNeedAddIdentityApi.Length != 0)
            {
                await CommandExecute(usersNeedAddIdentityApi, MicroServicesEnum.IdentityServer, connection);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }
    
    private readonly string _query = $@"
WITH required_services AS (
    SELECT unnest(@Microservices) AS micro_service
)

SELECT u.{nameof(HowUser.Id).ToSnake()}, rs.*
FROM {nameof(ApplicationDbContext.Users).ToSnake()} u 
LEFT JOIN required_services rs ON true
LEFT JOIN {nameof(ApplicationDbContext.UserMicroservices).ToSnake()} ums ON 
    u.{nameof(HowUser.Id).ToSnake()} = ums.{nameof(UserMicroservices.UserId).ToSnake()} 
        AND 
    rs.micro_service = ums.{nameof(UserMicroservices.MicroService).ToSnake()}
WHERE 
    ums.{nameof(UserMicroservices.ConfirmExisting).ToSnake()} = FALSE 
   OR 
    ums.{nameof(UserMicroservices.ConfirmExisting).ToSnake()} IS NULL
ORDER BY u.{nameof(HowUser.Id).ToSnake()};
";

    private readonly string _command = $@"
INSERT INTO user_microservices (user_id, micro_service, confirm_existing) 
VALUES ({nameof(_command)})
ON CONFLICT (user_id, micro_service) DO NOTHING;
";

    private async Task CommandExecute(int[] userIds, MicroServicesEnum target, NpgsqlConnection connection)
    {
        var boolColl = target == MicroServicesEnum.IdentityServer ? "TRUE" : "FALSE";
        var sequence = string.Join(",\n", userIds.Select(x => $"({x}, {(int)target}, {boolColl})"));
                
        var command = _command.Replace($"({nameof(_command)})", sequence);
                
        await connection.ExecuteAsync(command);
    }
}

// var query = $@"
// WITH cte AS (
//     SELECT
//         id AS id,
//         array_agg(umc.micro_service) AS ms_ids
//     FROM users u
//         LEFT JOIN user_microservices umc ON u.id = umc.user_id
//     WHERE u.is_deleted = FALSE AND u.is_suspended = FALSE
//     GROUP BY u.id
// )
//
// SELECT * 
// FROM cte
// WHERE 
//     ms_ids IS NULL
//    OR 
//     NOT (COALESCE(ms_ids, ARRAY[]::integer[])) @> @my_array;
// ";
//
// var secondQuery = $@"
// WITH required_services AS (
//     SELECT unnest(ARRAY[1, 2]) AS micro_service
// )
//
// SELECT u.id, array_agg(umc.micro_service) AS ms_ids
// FROM users u
//     LEFT JOIN user_microservices umc ON u.id = umc.user_id
//     LEFT JOIN required_services rs ON umc.micro_service = rs.micro_service
// WHERE u.is_deleted = FALSE
//   AND u.is_suspended = FALSE
// GROUP BY u.id
// HAVING COUNT(DISTINCT rs.micro_service) < (SELECT COUNT(*) FROM required_services)
//     OR COUNT(rs.micro_service) = 0;
// ";