using Msh.Api.Infra.Identity;

namespace Msh.Api.Infra.Security;

public interface ITokenService
{
    int TokenExpiresInHours { get; }
    int RefreshTokenExpiresInDays { get; }

    void AppendAuthCookies(HttpContext context, string accessToken, string refreshToken, ApplicationUser user);
    Task<string> GenerateAccessToken(ApplicationUser user);
    string GenerateRefreshToken();
}
