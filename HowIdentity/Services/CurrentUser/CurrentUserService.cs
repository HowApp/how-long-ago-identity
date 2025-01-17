namespace HowIdentity.Services.CurrentUser;

using System.Security.Claims;
using IdentityModel;

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
            if (_httpContextAccessor.HttpContext == null)
            {
                throw new NullReferenceException("HttpContext is null");
            }
            
            if (!Int32.TryParse(_httpContextAccessor.HttpContext.User.FindFirstValue(JwtClaimTypes.Subject), out var userId))
            {
                userId = -1;
            }

            return userId;
        }
    }
}