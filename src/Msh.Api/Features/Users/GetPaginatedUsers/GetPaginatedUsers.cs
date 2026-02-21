using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Msh.Api.Core.Commom;
using System.Linq;
using Msh.Api.Infra.Identity;

namespace Msh.Api.Features.Users.GetPaginatedUsers;

public record GetPaginatedUsersQuery(
    string? Search,
    int Page = 1,
    int PageSize = 10);

public record UserResponse(string Id, string Username, string Email, string Tipo, bool SessionActive);

public class GetPaginatedUsers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(EndpointConfig.RouteUsers, async ([AsParameters] GetPaginatedUsersQuery queryParams,
                                                                 CancellationToken ct,
                                                                 GetPaginatedUsersHandler handler) =>
        {
            return await handler.HandleGetPaginatedUsersAsync(queryParams, ct);
        })
        .WithTags(EndpointConfig.TagUsers)
        .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}

public class GetPaginatedUsersHandler(UserManager<ApplicationUser> userManager)
{
    public async Task<IResult> HandleGetPaginatedUsersAsync(GetPaginatedUsersQuery queryParams, CancellationToken ct)
    {
        var query = userManager.Users
                               .AsNoTracking()
                               .AsQueryable();

        // Busca por Nome ou Email
        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            query = query.Where(u => u.UserName!.Contains(queryParams.Search) || u.Email!.Contains(queryParams.Search));
        }

        var totalItems = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalItems / (double)queryParams.PageSize);

        var usuarios = await query.OrderBy(u => u.UserName)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(u => new UserResponse(
                u.Id,
                u.UserName ?? "",
                u.Email ?? "",
                u.Type.ToString(),
                // Aqui está a mágica: verificamos se existe algum token válido
                u.UserRefreshTokens.Any(t => !t.IsUsed && !t.IsRevoked && t.ExpiryDate > DateTime.UtcNow)
            ))
            .ToListAsync(ct);

        return Results.Ok(new PagedResponse<UserResponse>(usuarios, totalPages, queryParams.Page));
    }
}
