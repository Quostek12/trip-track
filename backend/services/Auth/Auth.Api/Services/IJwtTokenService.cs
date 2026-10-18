using System.Security.Claims;
using Auth.Api.Models;

namespace Auth.Api.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, IList<string> roles, out string jti);
    string GenerateRefreshToken();

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}