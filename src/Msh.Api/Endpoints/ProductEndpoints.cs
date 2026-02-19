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
        var group = app.MapGroup("/api/products")
                       .WithTags("Products")
                       .RequireRateLimiting("buscas"); 

        group.MapGet("/", HandleSearchProducts)
             .WithSummary("Search Products")
             .WithDescription("Searches via Meilisearch with HybridCache.");

        group.MapGet("/{produto}", HandleSearchByName)
             .WithSummary("Search Products by Name");

        return app;
    }

    private static async Task<IResult> HandleSearchProducts(
        ISearchProvider provider,
        HybridCache cache,
        [AsParameters] SearchProductRequest request,
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

    private static async Task<IResult> HandleSearchByName(
        string produto,
        ISearchProvider provider, // Supondo que você usará o provedor real
        CancellationToken ct)
    {
        // Exemplo de lógica real:
        // var result = await provider.FindByNameAsync(produto, ct);
        // if (result is null) return TypedResults.NotFound();

        var p = new ProductResponse(
            Id: "088590",
            Name: produto,
            Barcode: "123456789",
            Brand: "BrandX",
            Price: 99.99m,
            Stock: 10,
            List: "ListA"
        );

        return TypedResults.Ok(p);
    }
}
