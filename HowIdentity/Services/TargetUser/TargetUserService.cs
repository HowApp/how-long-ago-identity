namespace HowIdentity.Services.TargetUser;

using Common.Configurations;
using CurrentUser;
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

    public async Task<bool> AccessIfTargetUserIsSuperAdmin(int targetId, NpgsqlConnection connection)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return false;
        }
    }
}