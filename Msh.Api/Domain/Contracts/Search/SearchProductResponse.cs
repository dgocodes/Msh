namespace Msh.Api.Domain.Contracts.Search;

public record SearchProductResponse(
    IReadOnlyList<ProductResponse> Items,
    IReadOnlyList<FacetResponse> Facets,
    int CurrentPage,
    int PageSize,
    long TotalCount,
    int ProcessingTimeMs);
