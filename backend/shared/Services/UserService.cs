using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Shared.Services;

public class UserService : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    private HttpContext? HttpContext => _httpContextAccessor.HttpContext;

    public Guid? UserId
    {
        get
        {
            if (HttpContext == null) return null;
            if (HttpContext.Request.Headers.TryGetValue("X-User-Id", out var headerUserId) &&
                Guid.TryParse(headerUserId, out var parsedGuid))
            {
                return parsedGuid;
            }
            return null;
        }
    }

    public IEnumerable<string> Roles
    {
        get
        {
            if (HttpContext == null) return Enumerable.Empty<string>();
            if (HttpContext.Request.Headers.TryGetValue("X-User-Roles", out var headerRoles))
            {
                return headerRoles.ToString().Split(',',StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }

            return Enumerable.Empty<string>();
        }
    }
    
    public bool IsAuthenticated => UserId.HasValue;

    public bool IsInRole(string roleName) => Roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
}