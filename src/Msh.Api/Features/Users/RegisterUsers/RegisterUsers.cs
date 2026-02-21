using Microsoft.AspNetCore.Identity;
using Msh.Api.Core.Commom;
using Msh.Api.Core.Enums;
using Msh.Api.Infra.Identity;

namespace Msh.Api.Features.Users.RegisterUsers;

public record RegisterUserCommad(string Email, string Password, string Nome, string ErpId, EUserType Tipo);

public class RegisterUsers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(EndpointConfig.RouteUsers, async (RegisterUserCommad command,
                                                      CancellationToken ct,
                                                      CreateUsersHandler handler) =>
        {
            return await handler.CreateUsersAsync(command, ct);
        })
        .WithTags(EndpointConfig.TagUsers)
        .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}

public class CreateUsersHandler(UserManager<ApplicationUser> userManager)
{
    public async Task<IResult> CreateUsersAsync(RegisterUserCommad command, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            UserName = command.Email,

            Email = command.Email,
            FullName = command.Nome,
            ErpId = command.ErpId,
            Type = command.Tipo,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, command.Password);
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    }
}
