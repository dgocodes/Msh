using System.Text.Json.Serialization;
using Msh.Api.Core.Commom;

namespace Msh.Api.Features.Products.SearchProduts;

public record SearchProductResponse(
    IReadOnlyList<ProductResponse> Items,
    IReadOnlyList<FacetResponse> Facets,
    int CurrentPage,
    int PageSize,
    long TotalCount,
    int ProcessingTimeMs);

public record ProductResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("nome")] string Name,
    [property: JsonPropertyName("codigo_barras")] string Barcode,
    [property: JsonPropertyName("marca")] string Brand,
    [property: JsonPropertyName("lista")] string List,
    [property: JsonPropertyName("preco")] decimal Price,
    [property: JsonPropertyName("estoque")] int Stock);

public record FacetResponse(string Facet, List<FacetItemResponse> Options);

public record FacetItemResponse
{
    public string Description { get; init; } = string.Empty;
    public int Quantity { get; set; }
    public string ApplyLink { get; init; } = string.Empty;
    public bool Selected { get; set; }
    public List<FacetItemResponse> SubLevels { get; init; } = [];

    public static FacetItemResponse Create(
        string description,
        int quantity,
        HashSet<string>? activeFilters = null)
    {
        var applyLink = StringExtensions.Sanitize(description);
        return new FacetItemResponse
        {
            Description = description,
            Quantity = quantity,
            ApplyLink = applyLink,
            Selected = activeFilters?.Contains(applyLink) ?? false
        };
    }
}
