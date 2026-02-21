using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Msh.Api.Core.Entities;
using Msh.Api.Infra.Identity;

namespace Msh.Api.Infra.Security;

public class TokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager) : ITokenService
{
    public int TokenExpiresInHours => 10;
    public int RefreshTokenExpiresInDays => 10;

    public void AppendAuthCookies(HttpContext context, string accessToken, string refreshToken, ApplicationUser user)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(TokenExpiresInHours)
        };

        context.Response.Cookies.Append("token", accessToken, cookieOptions);

        // O Cookie do userType é acessível via JavaScript para que o frontend possa ler e ajustar a UI conforme o tipo do usuário
        cookieOptions.HttpOnly = false;
        context.Response.Cookies.Append("userType", user.Type.ToString(), cookieOptions);
        context.Response.Cookies.Append("username", user?.UserName ?? "", cookieOptions);
        context.Response.Cookies.Append("erpId", user?.ErpId ?? "", cookieOptions);

        context.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            // O Cookie do Refresh dura muito mais (ex: 10 dias)
            Expires = DateTime.UtcNow.AddDays(RefreshTokenExpiresInDays)
        });
    }

    public async Task<string> GenerateAccessToken(ApplicationUser user)
    {
        var secretKey = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException("JWT Key not configured"));

        // 1. Criamos a lista base de Claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new("erp_id", user.ErpId),
            new("user_type", user.Type.ToString())
        };

        // 2. Buscamos as Roles no banco e adicionamos à lista
        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // 3. Montamos o Descriptor usando a lista de claims
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims), 
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"],
            Expires = DateTime.UtcNow.AddHours(TokenExpiresInHours),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(secretKey),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}