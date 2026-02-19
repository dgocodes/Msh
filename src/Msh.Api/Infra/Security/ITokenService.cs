using Msh.Api.Infra.Identity;

namespace Msh.Api.Infra.Security;

public interface ITokenService
{
    Task<string> GenerateAccessToken(ApplicationUser user);
    string GenerateRefreshToken();
}
