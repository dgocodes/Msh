using Microsoft.AspNetCore.Mvc;
using Msh.Api.Core.Abstractions;
using Msh.Api.Core.Commom;

namespace Msh.Api.Features.Products.GetProductById;

public class GetProductsById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(EndpointConfig.RouteProductById, async (string id, CancellationToken ct, GetProductsByIdHandler handler) =>
        {
            return await handler.HandleAsync(id, ct);
        }).WithTags(EndpointConfig.TagProducts);
    }
}

public class GetProductsByIdHandler(ISearchProvider provider)
{
    public async Task<IResult> HandleAsync(string idProduto,
                                           CancellationToken ct)
    {
        return TypedResults.Ok(await provider.GetDocumentAsync(idProduto, ct));
    }
}
