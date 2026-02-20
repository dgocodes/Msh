using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Msh.Api.Infra.Identity;

namespace Msh.Api.Endpoints.Admin;

public static class AdminUsersEndpoints
{
    public record UserResponse(string Id, string Username, string Email, string Tipo);

    public static IEndpointRouteBuilder MapAdminUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapGet("", async (UserManager<ApplicationUser> userManager,
                                string? search,
                                int page = 1,
                                int pageSize = 10) =>
        {
            var query = userManager.Users.AsQueryable();

            // Busca por Nome ou Email
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.UserName!.Contains(search) || u.Email!.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var usuarios = await query.OrderBy(u => u.UserName)
                                      .Skip((page - 1) * pageSize) // Pula os registros das páginas anteriores
                                      .Take(pageSize)
                                      .Select(u => new UserResponse(u.Id, u.UserName ?? "", u.Email ?? "", u.Type.ToString()))
                                      .ToListAsync();

            return Results.Ok(new { usuarios, totalPages, currentPage = page });
        });
        //.RequireAuthorization(policy => policy.RequireRole("Admin"));

        return app;
    }
}