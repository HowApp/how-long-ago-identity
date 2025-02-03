namespace HowIdentity.Infrastructure.Scheduler.Jobs;

using Data;
using Entity;
using HowCommon.Extensions;
using Processing.Producer;
using Quartz;

public class TestJob : IJob
{
    private readonly ILogger<TestJob> _logger;
    private readonly DapperContext _context;
    private readonly UserServiceAccountProducer _producer;

    public TestJob(ILogger<TestJob> logger, DapperContext context, UserServiceAccountProducer producer)
    {
        _logger = logger;
        _context = context;
        _producer = producer;
    }

    public Task Execute(IJobExecutionContext context)
    {
        var query = $@"
WITH cte AS (
    SELECT
        id AS id,
        array_agg(umc.micro_service) AS ms_ids
    FROM users u
             LEFT JOIN user_microservices umc ON u.id = umc.user_id
    WHERE u.is_deleted = FALSE AND u.is_suspended = FALSE
    GROUP BY u.id
)

SELECT * 
FROM cte
WHERE 
    ms_ids IS NULL
   OR 
    NOT (COALESCE(ms_ids, ARRAY[]::integer[])) @> @my_array;
";

        var secondQuery = $@"
WITH required_services AS (
    SELECT unnest(ARRAY[1, 2]) AS micro_service
)

SELECT u.id, array_agg(umc.micro_service) AS ms_ids
FROM users u
    LEFT JOIN user_microservices umc ON u.id = umc.user_id
    LEFT JOIN required_services rs ON umc.micro_service = rs.micro_service
WHERE u.is_deleted = FALSE
  AND u.is_suspended = FALSE
GROUP BY u.id
HAVING COUNT(DISTINCT rs.micro_service) < (SELECT COUNT(*) FROM required_services)
    OR COUNT(rs.micro_service) = 0;
";
        
        return Task.CompletedTask;
    }
}