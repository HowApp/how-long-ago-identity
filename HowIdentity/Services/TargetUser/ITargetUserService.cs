namespace HowIdentity.Services.TargetUser;

using Npgsql;

public interface ITargetUserService
{
    Task<bool> AccessIfTargetUserIsSuperAdmin(int targetId, NpgsqlConnection connection);
}