namespace HowIdentity.Services.CurrentUser;

using System.Security.Claims;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public int UserId
    {
        get
        {
            if (!Int32.TryParse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                userId = -1;
            }

            return userId;
        }
    }
}