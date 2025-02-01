namespace HowIdentity.Services.SuperAdmin;

using System.Text;
using Common.Configurations;
using Common.Constants;
using Common.ResultType;
using Dapper;
using Data;
using Duende.IdentityServer.Services;
using Entity;
using HowCommon.Extensions;
using Infrastructure.Processing.Producer;
using Microsoft.Extensions.Options;
using Npgsql;
using Pages.SuperAdmin.AppUsers;
using TargetUser;

public class SuperAdminUserService : ISuperAdminUserService
{
    private readonly ILogger<SuperAdminUserService> _logger;
    private readonly DapperContext _dapper;
    private readonly ISessionManagementService _sessionManagementService;
    private readonly ITargetUserService _targetUserService;
    private readonly IOptions<AdminCredentials> _adminCredentials;
    private readonly UserServiceAccountProducer _producer;

    public SuperAdminUserService(
        ILogger<SuperAdminUserService> logger,
        DapperContext dapper,
        ISessionManagementService sessionManagementService,
        ITargetUserService targetUserService,
        IOptions<AdminCredentials> adminCredentials,
        UserServiceAccountProducer producer)
    {
        _logger = logger;
        _dapper = dapper;
        _sessionManagementService = sessionManagementService;
        _targetUserService = targetUserService;
        _adminCredentials = adminCredentials;
        _producer = producer;
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
WHERE u.{nameof(HowUser.Email).ToSnake()} NOT ILIKE '{_adminCredentials.Value.Email}'
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
    AND u.{nameof(HowUser.Email).ToSnake()} NOT ILIKE '{_adminCredentials.Value.Email}'
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
            await using var connection = _dapper.InitConnection();
            
            if (!await _targetUserService.AccessToTargetUser(targetId: userId, connection: connection, protectSuperAdmin: true, preventSelfAction: true))
            {
                return ResultDefault.Fatality(nameof(DeleteUser), "User can not be suspended!");
            }
            
            var result = await UpdateSuspendStatus(userId, true, connection);

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

                await _producer.PublishUserSuspendMessage(userId, true);
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
            await using var connection = _dapper.InitConnection();
            
            if (!await _targetUserService.AccessToTargetUser(targetId: userId, connection: connection, protectSuperAdmin: true, preventSelfAction: true))
            {
                return ResultDefault.Fatality(nameof(DeleteUser), "User can not be re-suspended!");
            }
            
            var result = await UpdateSuspendStatus(userId, false, connection);

            if (result)
            {
                await _producer.PublishUserSuspendMessage(userId, false);
            }
            
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

    public async Task<ResultDefault> UpdateUserRoles(int userId, (int RoleId, bool State)[] roles, bool forceSessionRemoving = false)
    {
        try
        {
            if (roles.Length == 0)
            {
                return ResultDefault.Success();
            }

            await using var connection = _dapper.InitConnection();

            if (!await _targetUserService.AccessToTargetUser(targetId: userId, connection: connection, protectSuperAdmin: false, preventSelfAction: false))
            {
                return ResultDefault.Fatality(nameof(DeleteUser), "User can not edit roles!");
            }

            var roleToAdd = roles.Where(r => r.State).Select(r => r.RoleId).ToArray();
            var roleToDelete = roles.Where(r => !r.State && r.RoleId != IdentityRoleConstant.Role.SuperAdmin.Id).Select(r => r.RoleId).ToArray();
            var commandBuilder = new StringBuilder();

            if (roleToDelete.Any())
            {
                var commandDelete = $@"
DELETE FROM {nameof(ApplicationDbContext.UserRoles).ToSnake()}
WHERE {nameof(HowUserRole.RoleId).ToSnake()} = ANY(@RoleIdToDelete)
    AND {nameof(HowUserRole.UserId).ToSnake()} = @UserId;
";

                commandBuilder.Append(commandDelete);
            }

            if (roleToAdd.Any())
            {
                var sequence = string.Join(",\n", roleToAdd.Select(r => $"({userId}, {r})"));

                var commandInsert = $@"
INSERT INTO {nameof(ApplicationDbContext.UserRoles).ToSnake()} ({nameof(HowUserRole.UserId).ToSnake()}, {nameof(HowUserRole.RoleId).ToSnake()})
VALUES 
({nameof(sequence)}, {nameof(sequence)})
ON CONFLICT ({nameof(HowUserRole.UserId).ToSnake()}, {nameof(HowUserRole.RoleId).ToSnake()})
DO NOTHING;";

                commandBuilder.Append(commandInsert.Replace($"({nameof(sequence)}, {nameof(sequence)})", sequence));
            }

            var command = commandBuilder.ToString();

            await connection.ExecuteAsync(
                command, 
                new
                {
                    UserId = userId,
                    RoleIdToDelete = roleToDelete
                });

            if (forceSessionRemoving)
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

            return ResultDefault.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return ResultDefault.Fatality(nameof(UpdateUserRoles));
        }
    }

    public async Task<ResultDefault> DeleteUser(int userId)
    {
        try
        {
            await using var connection = _dapper.InitConnection();

            if (!await _targetUserService.AccessToTargetUser(targetId: userId, connection: connection, protectSuperAdmin: true, preventSelfAction: true))
            {
                return ResultDefault.Fatality(nameof(DeleteUser), "User can not be deleted!");
            }
            
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
    AND {nameof(HowUser.IsDeleted).ToSnake()} = FALSE
RETURNING *;
";

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

            await _producer.PublishUserDeletedMessage(userId);

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
        bool suspend,
        NpgsqlConnection connection)
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
                AND ur.{nameof(HowUserRole.RoleId).ToSnake()} IN ({IdentityRoleConstant.Role.SuperAdmin.Id}, {IdentityRoleConstant.Role.Admin.Id})
        )
    AND {nameof(HowUser.IsDeleted).ToSnake()} = FALSE
RETURNING *;
";

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