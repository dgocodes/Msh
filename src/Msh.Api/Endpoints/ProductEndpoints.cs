using Msh.Api.Domain.Contracts.Search;
using Msh.Api.Domain.Interfaces.Providers;

namespace Msh.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/products", SearchProducts)
           .WithName("SearchProducts")
           .WithTags("Products")
           .WithSummary("Search Products")
           .WithDescription("Searches for products based on a query and optional filters on provider Meilisearch.");

        return app;
    }

    private static async Task<IResult> SearchProducts(
        ISearchProvider provider,
        [AsParameters] SearchProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await provider.SearchAsync(request, cancellationToken);
        return TypedResults.Ok(result);
    }
}
