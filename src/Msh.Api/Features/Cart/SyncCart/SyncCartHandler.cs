using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msh.Api.Core.Commom;
using Msh.Api.Core.Entities;
using Msh.Api.Infra.Context;

namespace Msh.Api.Features.Cart.SyncCart;


public record SyncCartRequest(List<CartItemRequest> Items);
public record CartItemRequest(Guid SkuId, int Quantity);

public class SyncCartEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cart/sync", async (
             [FromBody] SyncCartRequest request,
             [FromServices] SyncCartHandler handler,
             ClaimsPrincipal user) =>
        {
            // Pega o ID do usuário logado via Token JWT
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            await handler.Handle(userId, request);

            return Results.Ok(new { message = "Carrinho sincronizado com sucesso" });
        })
         .WithName("SyncCart")
         .WithTags("Cart")
         .RequireAuthorization(); // Garante que só logados acessem
    }
}


public class SyncCartHandler(MshDbContext context)
{
    public async Task Handle(string userId, SyncCartRequest request)
    {
        // 1. Busca o carrinho atual do usuário no banco (se existir)
        var dbCart = await context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (dbCart == null)
        {
            dbCart = new Core.Entities.Cart { UserId = userId };
            context.Carts.Add(dbCart);
        }

        // 2. Lógica de Merge
        foreach (var anonymousItem in request.Items)
        {
            var existingItem = dbCart.Items
                .FirstOrDefault(i => i.SkuId == anonymousItem.SkuId);

            if (existingItem != null)
            {
                // Se já existe, soma a quantidade
                existingItem.Quantity += anonymousItem.Quantity;
            }
            else
            {
                // Se é novo, adiciona ao carrinho do banco
                dbCart.Items.Add(new CartItem
                {
                    SkuId = anonymousItem.SkuId,
                    Quantity = anonymousItem.Quantity
                });
            }
        }

        await context.SaveChangesAsync();
    }
}