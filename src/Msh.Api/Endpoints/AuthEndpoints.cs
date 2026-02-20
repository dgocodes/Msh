using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Msh.Api.Domain.Entities;
using Msh.Api.Infra.Context;
using Msh.Api.Infra.Identity;
using Msh.Api.Infra.Security;

namespace Msh.Api.Endpoints;

public record RegisterDTO(string Email, string Password, string Nome, string ErpId, EUserType Tipo);
public record LoginDTO(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", HandleRegister).WithSummary("Register new user");
        group.MapPost("/login", HandleLogin).WithSummary("Login user");
        group.MapPost("/refresh", HandleRefreshToken).WithSummary("Refresh Token");
        group.MapPost("/logout", () => Results.Ok()).WithSummary("Logout");

        return app;
    }

    private static async Task<IResult> HandleRegister(RegisterDTO model, UserManager<ApplicationUser> userManager)
    {
        var user = new ApplicationUser
        {
            FullName = model.Email,
            ErpId = model.ErpId,    
            Type = model.Tipo,
        };

        var result = await userManager.CreateAsync(user, model.Password);
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleLogin(LoginDTO model,
                                                   UserManager<ApplicationUser> userManager,
                                                   ITokenService tokenService,
                                                   MshDbContext context,
                                                   HttpContext httpContext)
    {
        var user = await userManager.FindByEmailAsync(model.Email);

        if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
            return Results.Unauthorized();

        var accessToken = await tokenService.GenerateAccessToken(user);
        var refreshTokenValue = tokenService.GenerateRefreshToken();

        // Salva o Refresh Token no banco
        var refreshToken = new UserRefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7), // Dura 7 dias
            IsUsed = false,
            IsRevoked = false
        };

        context.UserRefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        // Configuração para os Tokens (Segurança Máxima)
        var tokenOptions = new CookieOptions
        {
            HttpOnly = true,   // Protege contra roubo via JavaScript (XSS)
            Secure = true,     // Exige HTTPS
            SameSite = SameSiteMode.Strict, // Protege contra CSRF
            Expires = DateTime.UtcNow.AddHours(1) // Expiração curta para o Access Token
        };

        // Configuração para o Refresh Token (Expiração Longa)
        var refreshOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7) // Dura mais tempo
        };

        // Configuração para o Tipo de Usuário (Opcional: HttpOnly = false)
        var infoOptions = new CookieOptions
        {
            HttpOnly = false,  // Deixe como false se você precisar ler isso no Front-end via JS
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(1)
        };

        // Enviando os Cookies
        httpContext.Response.Cookies.Append("token", accessToken, tokenOptions);
        httpContext.Response.Cookies.Append("refreshToken", refreshTokenValue, refreshOptions);
        httpContext.Response.Cookies.Append("userType", user.Type.ToString(), infoOptions);

        return Results.Ok(new
        {
            Token = accessToken,
            RefreshToken = refreshTokenValue, 
            Tipo = user.Type.ToString()
        });
    }

    private static async Task<IResult> HandleRefreshToken(RefreshTokenRequest request,
                                                          MshDbContext context,
                                                          ITokenService tokenService,
                                                          UserManager<ApplicationUser> userManager)
    {
        // 1. Procura o token no banco
        var storedToken = await context.UserRefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (storedToken == null || storedToken.IsUsed || storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow)
            return Results.Unauthorized();

        // 2. Busca o usuário
        var user = await userManager.FindByIdAsync(storedToken.UserId);
        if (user == null) return Results.Unauthorized();

        // 3. Marca o token antigo como usado e gera novos
        storedToken.IsUsed = true;

        var newAccessToken = tokenService.GenerateAccessToken(user);
        var newRefreshTokenValue = tokenService.GenerateRefreshToken();

        var newRefreshToken = new UserRefreshToken
        {
            Token = newRefreshTokenValue,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsUsed = false
        };

        context.UserRefreshTokens.Add(newRefreshToken);
        await context.SaveChangesAsync();

        return Results.Ok(new { Token = newAccessToken, RefreshToken = newRefreshTokenValue });
    }
}