namespace HowIdentity.Services.SuperAdmin;

using Common.Constants;
using Common.Extensions;
using Dapper;
using Data;
using Duende.IdentityServer.Services;
using Entity;

public class SuperAdminUserService : ISuperAdminUserService
{
    private readonly ILogger<SuperAdminUserService> _logger;
    private readonly DapperContext _dapper;
    private readonly ISessionManagementService _sessionManagementService;

    public SuperAdminUserService(
        ILogger<SuperAdminUserService> logger,
        DapperContext dapper,
        ISessionManagementService sessionManagementService)
    {
        _logger = logger;
        _dapper = dapper;
        _sessionManagementService = sessionManagementService;
    }

    public async Task<(
        List<(int Id, string Email, string Roles, bool IsSuspended, bool IsDeleted)> Values,
        bool Success,
        (string KeyError, string MessageError) Error
        )> GetUsers()
    {
        try
        {
            var query = $@"
SELECT 
    {nameof(HowUser.Id).ToSnake()},
    {nameof(HowUser.Email).ToSnake()},
    user_roles.roles,
    {nameof(HowUser.IsSuspended).ToSnake()},
    {nameof(HowUser.IsDeleted).ToSnake()}
FROM {nameof(ApplicationDbContext.Users).ToSnake()} u
LEFT JOIN (
        SELECT 
        ur.{nameof(HowUserRole.UserId).ToSnake()} AS user_id,
        STRING_AGG(r.{nameof(HowRole.Name).ToSnake()}, '; ' ) AS roles
    FROM {nameof(ApplicationDbContext.UserRoles).ToSnake()} ur
    LEFT JOIN {nameof(ApplicationDbContext.Roles).ToSnake()} r on r.{nameof(HowRole.Id).ToSnake()} = ur.{nameof(HowUserRole.RoleId).ToSnake()}
    GROUP BY ur.{nameof(HowUserRole.UserId).ToSnake()}
) user_roles ON u.{nameof(HowUser.Id).ToSnake()} = user_roles.user_id
";
            await using var connection = _dapper.InitConnection();
            var users = await connection.QueryAsync<(int Id, string Email, string Roles, bool IsSuspended, bool IsDeleted)>(query);
            
            var result = users.ToList();
            
            return (result, result.Any(), (string.Empty, string.Empty));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return (new(),false, (nameof(GetUsers), e.Message));
        }
    }

    public async Task<(bool Success, (string KeyError, string MessageError) Error)> SuspendUser(int userId)
    {
        try
        {
            var result = await UpdateSuspendStatus(userId, true);

            if (result.Success)
            {
                await _sessionManagementService.RemoveSessionsAsync(new RemoveSessionsContext
                {
                    SubjectId = userId.ToString(),
                    RemoveServerSideSession = true,
                    SendBackchannelLogoutNotification = true,
                    RevokeTokens = true,
                    RevokeConsents = true
                });
            }

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return (false, (nameof(SuspendUser), e.Message));
        }
    }
    
    public async Task<(bool Success, (string KeyError, string MessageError) Error)> ReSuspendUser(int userId)
    {
        try
        {
            var result = await UpdateSuspendStatus(userId, false);
            
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return (false, (nameof(SuspendUser), e.Message));
        }
    }

    public async Task<(bool Success, (string KeyError, string MessageError) Error)> DeleteUser(int userId)
    {
        try
        {
            var salt = "Deleted_" + Guid.NewGuid();
            var command = $@"
UPDATE {nameof(ApplicationDbContext.Users).ToSnake()}
SET 
    {nameof(HowUser.UserName).ToSnake()} = @UserName || {nameof(HowUser.UserName).ToSnake()},
    {nameof(HowUser.NormalizedUserName).ToSnake()} = @NormalizedUserName || {nameof(HowUser.UserName).ToSnake()}, 
    {nameof(HowUser.Email).ToSnake()} = @Email,
    {nameof(HowUser.NormalizedEmail).ToSnake()} = @NormalizedEmail,
    {nameof(HowUser.IsSuspended).ToSnake()} = true,
    {nameof(HowUser.IsDeleted).ToSnake()} = true
WHERE {nameof(HowUser.Id).ToSnake()} = @Id 
    AND NOT EXISTS(
        SELECT 1
        FROM {nameof(ApplicationDbContext.UserRoles).ToSnake()} ur
        WHERE ur.{nameof(HowUserRole.UserId).ToSnake()} = @Id 
            AND ur.{nameof(HowUserRole.RoleId).ToSnake()} IN ({AppConstants.Role.SuperAdmin.Id}, {AppConstants.Role.Admin.Id})
    )
RETURNING *;
";
            
            await using var connection = _dapper.InitConnection();
            var result = await connection.ExecuteAsync(
                command, 
                new
                {
                    Id = userId,
                    UserName = salt,
                    NormalizedUserName = salt.ToUpper(),
                    Email = salt,
                    NormalizedEmail = salt.ToUpper()
                });

            if (result == 0)
            {
                return (false, (nameof(DeleteUser), "Something going wrong!"));
            }
            
            await _sessionManagementService.RemoveSessionsAsync(new RemoveSessionsContext
            {
                SubjectId = userId.ToString(),
                RemoveServerSideSession = true,
                SendBackchannelLogoutNotification = true,
                RevokeTokens = true,
                RevokeConsents = true
            });
            
            return (true, (string.Empty, string.Empty));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return (false, (nameof(DeleteUser), e.Message));
        }
    }

    private async Task<(bool Success, (string KeyError, string MessageError) Error)> UpdateSuspendStatus(
        int userId,
        bool suspend)
    {
        var command = $@"
UPDATE {nameof(ApplicationDbContext.Users).ToSnake()}
SET 
    {nameof(HowUser.IsSuspended).ToSnake()} = @IsSuspended
WHERE {nameof(HowUser.Id).ToSnake()} = @Id
    AND NOT EXISTS(
            SELECT 1
            FROM {nameof(ApplicationDbContext.UserRoles).ToSnake()} ur
            WHERE ur.{nameof(HowUserRole.UserId).ToSnake()} = @Id 
                AND ur.{nameof(HowUserRole.RoleId).ToSnake()} IN ({AppConstants.Role.SuperAdmin.Id}, {AppConstants.Role.Admin.Id})
        )
RETURNING *;
";
            
        await using var connection = _dapper.InitConnection();
        var result = await connection.ExecuteAsync(
            command,
            new
            {
                Id = userId,
                IsSuspended = suspend
            });

        if (result == 0)
        {
            return (false, (nameof(SuspendUser), "Something going wrong!"));
        }
        
        return (true, (string.Empty, string.Empty));
    }
}