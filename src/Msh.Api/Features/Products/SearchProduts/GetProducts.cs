using Microsoft.Extensions.Caching.Hybrid;
using Msh.Api.Core.Abstractions;
using Msh.Api.Core.Commom;
using Msh.Api.Extensions;

namespace Msh.Api.Features.Products.SearchProduts;

public class GetProducts : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(EndpointConfig.RouteProducts, async (
            [AsParameters] SearchProductRequest query,
            CancellationToken ct,
            SearchProductsHandler handler) =>
        {
            return await handler.HandleSearchProductsAsync(query, ct);
        }).WithTags(EndpointConfig.TagProducts);
    }
}

public class SearchProductsHandler(ISearchProvider provider,
                                   HybridCache cache)
{
    public async Task<IResult> HandleSearchProductsAsync(SearchProductRequest request,
                                                         CancellationToken ct)
    {
        // Gerar chave baseada nos campos da request para evitar colisões
        var cacheKey = $"products:search:{request.GetHashCode()}";

        var result = await cache.GetOrCreateAsync(
            cacheKey,
            async token => await provider.SearchAsync(request, token),
            options: HybridCacheExtensions.CreateOptions(2, 5),
            cancellationToken: ct
        );

        return TypedResults.Ok(result);
    }
}
