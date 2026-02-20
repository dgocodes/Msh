using Microsoft.AspNetCore.Identity;
using Msh.Api.Core.Commom;
using Msh.Api.Core.Entities;
using Msh.Api.Infra.Context;
using Msh.Api.Infra.Identity;
using Msh.Api.Infra.Security;

namespace Msh.Api.Features.Users.Login;

public record SignInCommand(string Email, string Password);

public class SignIn : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{EndpointConfig.RouteUsers}/login", async (
            SignInCommand command,
            SignInHandler handler,
            HttpContext httpContext, // Injetado automaticamente pelo ASP.NET
            CancellationToken ct) =>
        {
            return await handler.AuthenticateAsync(command, httpContext, ct);
        })
        .WithTags(EndpointConfig.TagUsers)
        .AllowAnonymous(); // Essencial para permitir que deslogados acessem
    }
}

public class SignInHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    MshDbContext context)
{
    public async Task<IResult> AuthenticateAsync(SignInCommand command, HttpContext httpContext, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(command.Email);

        if (user == null || !await userManager.CheckPasswordAsync(user, command.Password))
            return Results.Unauthorized();

        var accessToken = await tokenService.GenerateAccessToken(user);
        var refreshTokenValue = tokenService.GenerateRefreshToken();

        // 1. Salva o Refresh Token no banco
        var refreshToken = new UserRefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        context.UserRefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(ct);

        // 2. Configurações de Cookies
        var defaultCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Certifique-se de usar HTTPS em Prod
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(1)
        };

        // 3. Enviando os Cookies via HttpContext
        httpContext.Response.Cookies.Append("token", accessToken, defaultCookieOptions);

        httpContext.Response.Cookies.Append("refreshToken", refreshTokenValue, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        httpContext.Response.Cookies.Append("userType", user.Type.ToString(), new CookieOptions
        {
            HttpOnly = false, // Permitir que o Front-end leia o tipo do usuário
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(1)
        });

        return Results.Ok(new
        {
            Token = accessToken,
            RefreshToken = refreshTokenValue,
            Tipo = user.Type.ToString()
        });
    }
}