using Microsoft.Extensions.Caching.Hybrid;
using Msh.Api.Domain.Contracts.Search;
using Msh.Api.Domain.Core;
using Msh.Api.Domain.Interfaces.Providers;
using Msh.Api.Extensions;

namespace Msh.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", SearchProducts)
           .WithName("SearchProducts")
           .WithTags("Products")
           .WithSummary("Search Products")
           .WithDescription("Searches for products based on a query and optional filters on provider Meilisearch.");

        return app;
    }

    private static async Task<IResult> SearchProducts(
        ISearchProvider provider,
        HybridCache cache,
        CancellationToken cancellationToken,
        [AsParameters] SearchProductRequest request)
    {
        var cacheKey = StringExtensions.GenerateCacheKey(request.ToString());

        var result = await cache.GetOrCreateAsync(
            cacheKey,
            async token => await provider.SearchAsync(request, token), 
            options: HybridCacheExtensions.CreateOptions(2, 5),
            cancellationToken: cancellationToken
        );

        return TypedResults.Ok(result);
    }
}
