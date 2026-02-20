using Msh.Api.Core.Abstractions;
using Msh.Api.Features.Products.SearchProduts;

namespace Msh.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
                       .WithTags("Products")
                       .RequireRateLimiting("buscas"); 

        group.MapGet("/{produto}", HandleSearchByName)
             .WithSummary("Search Products by Name");

        return app;
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
