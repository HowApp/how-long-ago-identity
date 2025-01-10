namespace HowIdentity.Services.SuperAdmin;

using Common.Extensions;
using Dapper;
using Data;
using Entity;

public class SuperAdminUserService : ISuperAdminUserService
{
    private readonly ILogger<SuperAdminUserService> _logger;
    private readonly DapperContext _dapper;

    public SuperAdminUserService(ILogger<SuperAdminUserService> logger, DapperContext dapper)
    {
        _logger = logger;
        _dapper = dapper;
    }

    public async Task<(
        List<(int Id, string Email, bool IsSuspended, bool IsDeleted)> Values,
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
    {nameof(HowUser.IsSuspended).ToSnake()},
    {nameof(HowUser.IsDeleted).ToSnake()}
FROM {nameof(ApplicationDbContext.Users).ToSnake()} u
";
            await using var connection = _dapper.InitConnection();
            var users = await connection.QueryAsync<(int Id, string Email, bool IsSuspended, bool IsDeleted)>(query);
            
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
            var command = $@"
UPDATE {nameof(ApplicationDbContext.Users).ToSnake()}
SET 
    {nameof(HowUser.IsSuspended).ToSnake()} = true
WHERE {nameof(HowUser.Id).ToSnake()} = @userId
RETURNING *;
";
            
            await using var connection = _dapper.InitConnection();
            var result = await connection.ExecuteAsync(command);

            if (result == 0)
            {
                return (false, (nameof(SuspendUser), "Something going wrong!"));
            }
            
            return (true, (string.Empty, string.Empty));
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
    {nameof(HowUser.UserName).ToSnake()} = @UserName + {nameof(HowUser.UserName).ToSnake()},
    {nameof(HowUser.NormalizedUserName).ToSnake()} = @NormalizedUserName + {nameof(HowUser.UserName).ToSnake()}, 
    {nameof(HowUser.Email).ToSnake()} = @Email,
    {nameof(HowUser.NormalizedEmail).ToSnake()} = @NormalizedEmail,
    {nameof(HowUser.IsDeleted).ToSnake()} = true
WHERE {nameof(HowUser.Id).ToSnake()} = @userId
RETURNING *;
";
            
            await using var connection = _dapper.InitConnection();
            var result = await connection.ExecuteAsync(
                command, 
                new
                {
                    UserName = salt,
                    NormalizedUserName = salt.ToUpper(),
                    Email = salt,
                    NormalizedEmail = salt.ToUpper()
                });

            if (result == 0)
            {
                return (false, (nameof(DeleteUser), "Something going wrong!"));
            }
            
            return (true, (string.Empty, string.Empty));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return (false, (nameof(DeleteUser), e.Message));
        }
    }
}