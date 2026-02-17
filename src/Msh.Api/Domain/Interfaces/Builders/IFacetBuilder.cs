using Msh.Api.Domain.Contracts.Search;
using Msh.Api.Infra.Providers.Meili;

namespace Msh.Api.Domain.Interfaces.Builders;

public interface IFacetBuilder
{
    List<FacetResponse> Build(IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> resultFacets,
                              IReadOnlyList<FacetConfig> facetConfigs,
                              IReadOnlyList<string>? activeFilters);
}
