namespace HowIdentity.Services.SuperAdmin;

using Common.Constants;
using Common.Extensions;
using Common.ResultType;
using Dapper;
using Data;
using Duende.IdentityServer.Services;
using Entity;
using Pages.SuperAdmin.AppUsers;

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

    public async Task<ResultGeneric<List<AppUserModel>>>  GetUsers()
    {
        try
        {
            var query = $@"
SELECT 
    {nameof(HowUser.Id).ToSnake()} AS {nameof(AppUserModel.Id)},
    {nameof(HowUser.Email).ToSnake()} AS {nameof(AppUserModel.Email)},
    user_roles.roles_name AS {nameof(AppUserModel.Roles)},
    user_roles.roles_id AS {nameof(AppUserModel.RoleIds)},
    {nameof(HowUser.IsSuspended).ToSnake()} AS {nameof(AppUserModel.IsSuspended)},
    {nameof(HowUser.IsDeleted).ToSnake()} AS {nameof(AppUserModel.IsDeleted)}
FROM {nameof(ApplicationDbContext.Users).ToSnake()} u
LEFT JOIN (
        SELECT 
        ur.{nameof(HowUserRole.UserId).ToSnake()} AS user_id,
        ARRAY_AGG(r.{nameof(HowRole.Id).ToSnake()}) AS roles_id,
        STRING_AGG(r.{nameof(HowRole.Name).ToSnake()}, '; ' ) AS roles_name
    FROM {nameof(ApplicationDbContext.UserRoles).ToSnake()} ur
    LEFT JOIN {nameof(ApplicationDbContext.Roles).ToSnake()} r on r.{nameof(HowRole.Id).ToSnake()} = ur.{nameof(HowUserRole.RoleId).ToSnake()}
    GROUP BY ur.{nameof(HowUserRole.UserId).ToSnake()}
) user_roles ON u.{nameof(HowUser.Id).ToSnake()} = user_roles.user_id
";
            await using var connection = _dapper.InitConnection();
            var users = await connection.QueryAsync<AppUserModel>(query);
            
            return ResultGeneric<List<AppUserModel>>.Success(users.ToList());
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return ResultGeneric<List<AppUserModel>>.Fatality(nameof(GetUsers));
        }
    }

    public async Task<ResultGeneric<AppUserModel>> GetUserById(int userId)
    {
        try
        {
            var query = $@"
SELECT 
    {nameof(HowUser.Id).ToSnake()} AS {nameof(AppUserModel.Id)},
    {nameof(HowUser.Email).ToSnake()} AS {nameof(AppUserModel.Email)},
    user_roles.roles_name AS {nameof(AppUserModel.Roles)},
    user_roles.roles_id AS {nameof(AppUserModel.RoleIds)},
    {nameof(HowUser.IsSuspended).ToSnake()} AS {nameof(AppUserModel.IsSuspended)},
    {nameof(HowUser.IsDeleted).ToSnake()} AS {nameof(AppUserModel.IsDeleted)}
FROM {nameof(ApplicationDbContext.Users).ToSnake()} u
LEFT JOIN (
        SELECT 
        ur.{nameof(HowUserRole.UserId).ToSnake()} AS user_id,
        ARRAY_AGG(r.{nameof(HowRole.Id).ToSnake()}) AS roles_id,
        STRING_AGG(r.{nameof(HowRole.Name).ToSnake()}, '; ' ) AS roles_name
    FROM {nameof(ApplicationDbContext.UserRoles).ToSnake()} ur
    LEFT JOIN {nameof(ApplicationDbContext.Roles).ToSnake()} r on r.{nameof(HowRole.Id).ToSnake()} = ur.{nameof(HowUserRole.RoleId).ToSnake()}
    GROUP BY ur.{nameof(HowUserRole.UserId).ToSnake()}
) user_roles ON u.{nameof(HowUser.Id).ToSnake()} = user_roles.user_id
WHERE u.{nameof(HowUser.Id).ToSnake()} = @Id
    AND {nameof(HowUser.IsDeleted).ToSnake()} = FALSE
LIMIT 1;
";
            
            await using var connection = _dapper.InitConnection();
            var quryResult = await connection.QueryAsync<AppUserModel>(
                query,
                new { Id = userId });
            
            var user = quryResult.FirstOrDefault();

            if (user is null)
            {
                return ResultGeneric<AppUserModel>.Fatality(nameof(GetUserById), "User not found!");
            }
            
            return ResultGeneric<AppUserModel>.Success(user);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return ResultGeneric<AppUserModel>.Fatality(nameof(GetUserById));
        }
    }

    public async Task<ResultDefault> SuspendUser(int userId)
    {
        try
        {
            var result = await UpdateSuspendStatus(userId, true);

            if (result)
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

            return result ? 
                ResultDefault.Success() : 
                ResultDefault.Fatality(nameof(SuspendUser), "User is not suspended!");
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return ResultDefault.Fatality(nameof(SuspendUser));
        }
    }
    
    public async Task<ResultDefault> ReSuspendUser(int userId)
    {
        try
        {
            var result = await UpdateSuspendStatus(userId, false);
            
            return result ? 
                ResultDefault.Success() : 
                ResultDefault.Fatality(nameof(ReSuspendUser), "User is not resuspended!");
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return ResultDefault.Fatality(nameof(ReSuspendUser));
        }
    }

    public async Task<ResultDefault> DeleteUser(int userId)
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
    AND {nameof(HowUser.IsDeleted).ToSnake()} = FALSE
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
                return ResultDefault.Fatality(nameof(DeleteUser), "User not deleted!");
            }

            await _sessionManagementService.RemoveSessionsAsync(new RemoveSessionsContext
            {
                SubjectId = userId.ToString(),
                RemoveServerSideSession = true,
                SendBackchannelLogoutNotification = true,
                RevokeTokens = true,
                RevokeConsents = true
            });

            return ResultDefault.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return ResultDefault.Fatality(nameof(DeleteUser));
        }
    }

    private async Task<bool> UpdateSuspendStatus(
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
    AND {nameof(HowUser.IsDeleted).ToSnake()} = FALSE
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

        return result != 0;
    }
}