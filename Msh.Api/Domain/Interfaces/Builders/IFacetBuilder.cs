using Msh.Api.Domain.Contracts.Search;

namespace Msh.Api.Domain.Interfaces.Builders;

public interface IFacetBuilder
{
    List<FacetResponse> Build(IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> resultFacets,
                              Dictionary<string, string> facets,
                              IEnumerable<string>? activeFilters);
}
