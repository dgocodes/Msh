namespace Msh.Api.Domain.Contracts.Search;

public record FacetResponse(string Facet, List<FacetItemResponse> Options);
