namespace HowIdentity.Services.TargetUser;

using Npgsql;

public interface ITargetUserService
{
    Task<bool> AccessToTargetUser(
        int targetId,
        NpgsqlConnection connection,
        bool protectSuperAdmin = false,
        int? protectRole = null,
        bool preventSelfAction = false);
}