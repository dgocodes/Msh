using Microsoft.AspNetCore.Identity;
using Msh.Api.Core.Commom;
using Msh.Api.Core.Entities;
using Msh.Api.Infra.Context;
using Msh.Api.Infra.Identity;
using Msh.Api.Infra.Security;
using Microsoft.EntityFrameworkCore;

namespace Msh.Api.Features.Users.Login;

public class RefreshToken : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{EndpointConfig.RouteUsers}/refresh-token", async (
            RefreshTokenHandler handler,
            HttpContext httpContext, // Injetado automaticamente pelo ASP.NET
            CancellationToken ct) =>
        {
            return await handler.AuthenticateAsync(httpContext, ct);
        })
        .WithTags(EndpointConfig.TagUsers)
        .AllowAnonymous(); // Essencial para permitir que deslogados acessem
    }
}

public class RefreshTokenHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    MshDbContext context)
{
    public async Task<IResult> AuthenticateAsync(HttpContext httpContext, CancellationToken ct)
    {
        if (!httpContext.Request.Cookies.TryGetValue("refreshToken", out string? refreshToken))
            return Results.Unauthorized();

        // 1. Procura o token no banco
        var storedToken = await context.UserRefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken, ct);

        if (storedToken == null || storedToken.IsUsed || storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow)
            return Results.Unauthorized();

        // 2. Busca o usuário
        var user = await userManager.FindByIdAsync(storedToken.UserId);
        if (user == null) return Results.Unauthorized();

        // 3. Marca o token antigo como usado e gera novos
        storedToken.IsUsed = true;

        var newAccessToken = await tokenService.GenerateAccessToken(user);
        var newRefreshTokenValue = tokenService.GenerateRefreshToken();

        var newRefreshToken = new UserRefreshToken
        {
            Token = newRefreshTokenValue,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(tokenService.RefreshTokenExpiresInDays),
            IsUsed = false
        };

        tokenService.AppendAuthCookies(httpContext, newAccessToken, newRefreshTokenValue, user);

        context.UserRefreshTokens.Add(newRefreshToken);
        await context.SaveChangesAsync(ct);

        return Results.Ok(new { Token = newAccessToken, RefreshToken = newRefreshTokenValue });
    }
}