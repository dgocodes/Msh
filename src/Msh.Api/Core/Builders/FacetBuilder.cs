using Msh.Api.Core.Abstractions;
using Msh.Api.Features.Products.SearchProduts;
using Msh.Api.Infra.Providers.Meili;

namespace Msh.Api.Core.Builders;

public class FacetBuilder : IFacetBuilder
{
    public List<FacetResponse> Build(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> resultFacets,
        IReadOnlyList<FacetConfig> facetConfigs,
        IReadOnlyList<string>? activeFilters)
    {
        if (resultFacets == null || resultFacets.Count == 0)
            return [];

        var activeFiltersSet = activeFilters?.Any() == true
            ? new HashSet<string>(activeFilters, StringComparer.OrdinalIgnoreCase)
            : null;

        var facetsResult = new List<FacetResponse>(facetConfigs.Count);

        foreach (var config in facetConfigs)
        {
            if (resultFacets.TryGetValue(config.Key, out var items))
            {
                var mappedItems = MapFacetsToFilters(items, activeFiltersSet);

                if (mappedItems.Count > 0)
                {
                    facetsResult.Add(new FacetResponse(config.Label, mappedItems));
                }
            }
        }

        return facetsResult;
    }

    private static List<FacetItemResponse> MapFacetsToFilters(IReadOnlyDictionary<string, int> items, HashSet<string>? activeFilters)
    {
        var hierarchy = new Dictionary<string, FacetItemResponse>(items.Count);

        foreach (var item in items)
        {
            var parts = item.Key.Split('#');
            var parentName = parts[0];

            if (!hierarchy.TryGetValue(parentName, out var parent))
            {
                parent = FacetItemResponse.Create(parentName, 0, activeFilters);
                hierarchy[parentName] = parent;
            }

            if (parts.Length > 1)
            {
                var childName = parts[1];
                var child = FacetItemResponse.Create(childName, item.Value, activeFilters);
                parent.SubLevels.Add(child);
                parent.Quantity += item.Value;
                parent.Selected |= child.Selected;
            }
            else
            {
                parent.Quantity += item.Value;
            }
        }

        return hierarchy.Values
                        .OrderByDescending(x => x.Quantity)
                        .ToList();
    }
}