using Msh.Api.Core.Abstractions;
using Msh.Api.Core.Commom;
using Msh.Api.Core.Entities;
using Msh.Api.Extensions;
using Msh.Api.Infra.Context;

namespace Msh.Api.Features.Orders;

public class CreateOrder : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(EndpointConfig.RouteOrders, async (
            CreateOrderRequest request,
            HttpContext httpContext,
            CancellationToken ct,
            CreateOrderHandler handler) =>
        {
            return await handler.CreateOrderAsync(request, httpContext, ct);
        })
        .WithTags(EndpointConfig.TagOrders)
        .RequireAuthorization();
    }
}

public class CreateOrderHandler(ISearchProvider searchProvider, MshDbContext context)
{
    public async Task<IResult> CreateOrderAsync(
        CreateOrderRequest request,
        HttpContext httpContext,
        CancellationToken ct)
    {
        // Validar request
        if (!request.IsValid())
            return TypedResults.BadRequest(new { message = "Dados de pedido inválidos" });

        // Obter usuário autenticado
        var userId = httpContext.User.GetUserId();

        if (string.IsNullOrEmpty(userId))
            return TypedResults.Unauthorized();

        // Buscar preços dos produtos no Meilisearch
        var skuIds = request.Items.Select(i => i.SkuId).Distinct().ToList();
        var productPrices = await GetProductPricesAsync(skuIds, ct);

        // Validar se todos os SKUs foram encontrados
        var foundSkus = productPrices.Keys.ToHashSet();
        var missingSkus = skuIds.Except(foundSkus).ToList();

        if (missingSkus.Any())
        {
            return TypedResults.BadRequest(new
            {
                message = "Um ou mais produtos não foram encontrados",
                missingSkus = missingSkus
            });
        }

        // Criar pedido
        var order = new Order
        {
            UserId = userId,
            ShippingAddress = request.ShippingAddress.Trim(),
            Items = new()
        };

        decimal totalAmount = 0;

        // Adicionar items ao pedido
        foreach (var requestItem in request.Items)
        {
            var unitPrice = productPrices[requestItem.SkuId];
            var subTotal = requestItem.Quantity * unitPrice;

            var orderItem = new OrderItem
            {
                SkuId = requestItem.SkuId,
                Quantity = requestItem.Quantity,
                UnitPrice = unitPrice
            };

            order.Items.Add(orderItem);
            totalAmount += subTotal;
        }

        order.TotalAmount = totalAmount;

        // Persistir pedido
        context.Orders.Add(order);
        await context.SaveChangesAsync(ct);

        // Construir response
        var response = new CreateOrderResponse(
            OrderId: order.Id,
            Status: order.Status.ToString(),
            TotalAmount: order.TotalAmount,
            OrderDate: order.OrderDate,
            Items: order.Items.Select(item => new OrderItemResponse(
                ItemId: item.Id,
                SkuId: item.SkuId,
                Quantity: item.Quantity,
                UnitPrice: item.UnitPrice,
                SubTotal: item.SubTotal
            )).ToList()
        );

        return TypedResults.Created($"{EndpointConfig.RouteOrders}/{order.Id}", response);
    }

    /// <summary>
    /// Busca os preços dos produtos no Meilisearch.
    /// Retorna um dicionário com SKU -> Preço
    /// </summary>
    private async Task<Dictionary<string, decimal>> GetProductPricesAsync(
        List<string> skuIds,
        CancellationToken ct)
    {
        var prices = new Dictionary<string, decimal>();

        // Aqui você implementaria a lógica de busca individual ou em batch
        // por enquanto, exemplo com busca individual para cada SKU

        foreach (var skuId in skuIds)
        {
            // TODO: Implementar busca de preço no Meilisearch por SKU
            // Por exemplo, criar um SearchProductRequest com o SKU específico
            // e obter o preço do resultado

            // Placeholder: assumir preço padrão (REMOVER ANTES DE PRODUÇÃO)
            prices[skuId] = 100.00m;
        }

        return prices;
    }
}

