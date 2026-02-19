using System.Security.Claims;

namespace Msh.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetErpId(this ClaimsPrincipal user)
    {
        return user.FindFirst("erp_id")?.Value ?? string.Empty;
    }

    public static string GetUserType(this ClaimsPrincipal user)
    {
        return user.FindFirst("user_type")?.Value ?? string.Empty;
    }
}