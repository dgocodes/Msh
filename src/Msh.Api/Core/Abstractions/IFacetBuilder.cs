using Msh.Api.Features.Products.SearchProduts;
using Msh.Api.Infra.Providers.Meili;

namespace Msh.Api.Core.Abstractions;

public interface IFacetBuilder
{
    List<FacetResponse> Build(IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> resultFacets,
                              IReadOnlyList<FacetConfig> facetConfigs,
                              IReadOnlyList<string>? activeFilters);
}
