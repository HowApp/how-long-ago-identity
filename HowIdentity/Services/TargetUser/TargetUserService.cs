namespace HowIdentity.Services.TargetUser;

using Common.Configurations;
using CurrentUser;
using Dapper;
using Data;
using Entity;
using HowCommon.Extensions;
using Microsoft.Extensions.Options;
using Npgsql;

public class TargetUserService : ITargetUserService
{
    private readonly IOptions<AdminCredentials> _adminCredentials;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TargetUserService> _logger;

    public TargetUserService(
        IOptions<AdminCredentials> adminCredentials,
        ILogger<TargetUserService> logger,
        ICurrentUserService currentUserService)
    {
        _adminCredentials = adminCredentials;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<bool> AccessToTargetUser(
        int targetId,
        NpgsqlConnection connection,
        bool protectSuperAdmin = false,
        int? protectRole = null,
        bool preventSelfAction = false)
    {
        try
        {
            var authorId = _currentUserService.UserId;
           
            var query = $@"
WITH user_roles_aggregated AS (
        SELECT
            ur.{nameof(HowUserRole.UserId).ToSnake()} AS user_id,
            array_agg(ur.{nameof(HowUserRole.RoleId).ToSnake()}) AS roles
        FROM {nameof(ApplicationDbContext.UserRoles).ToSnake()} ur
        WHERE ur.{nameof(HowUserRole.UserId).ToSnake()} IN (@AuthorId, @TargetId)
        GROUP BY ur.{nameof(HowUserRole.UserId).ToSnake()}
    ),
     user_existence AS (
        SELECT 
            u.{nameof(HowUser.Id).ToSnake()} AS user_id,
            u.{nameof(HowUser.Email).ToSnake()} AS email
        FROM {nameof(ApplicationDbContext.Users).ToSnake()} u
        WHERE 
            u.{nameof(HowUser.Id).ToSnake()} IN (@AuthorId, @TargetId) AND 
            u.{nameof(HowUser.IsDeleted).ToSnake()} = FALSE
    )
SELECT
    COALESCE((SELECT roles FROM user_roles_aggregated WHERE user_id = @AuthorId), array[0]::integer[]) AS author_roles,
    COALESCE((SELECT roles FROM user_roles_aggregated WHERE user_id = @TargetId), array[0]::integer[]) AS target_roles,
    EXISTS(SELECT 1 FROM user_existence WHERE user_id = @AuthorId) AS author_exist,
    EXISTS(SELECT 1 FROM user_existence WHERE user_id = @TargetId) AS target_exist,
    (SELECT email FROM user_existence WHERE user_id = @AuthorId) AS author_email,
    (SELECT email FROM user_existence WHERE user_id = @TargetId) AS target_email;
";
            
            var queryResult = await connection.QueryFirstOrDefaultAsync<(
                int[] author_roles,
                int[] target_roles,
                bool authorExist,
                bool targetExist,
                string author_email,
                string target_email)>(
                query,
                new
                {
                    AuthorId = authorId,
                    TargetId = targetId
                });

            if (!queryResult.authorExist || !queryResult.targetExist)
            {
                var message = queryResult.authorExist
                    ? $"Target user does not exist! ID: {targetId}"
                    : $"Author user does not exist! ID: {authorId}";
                _logger.LogError(message);
                
                return false;
            }

            if (protectSuperAdmin && string.Equals(queryResult.target_email, _adminCredentials.Value.Email, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (protectRole.HasValue && queryResult.target_roles.Contains(protectRole.Value))
            {
                return false;
            }
            
            if (authorId == targetId && preventSelfAction)
            {
                return false;
            }

            if (queryResult.target_roles.DefaultIfEmpty(0).Max() > queryResult.author_roles.DefaultIfEmpty(0).Max())
            {
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return false;
        }
    }
}