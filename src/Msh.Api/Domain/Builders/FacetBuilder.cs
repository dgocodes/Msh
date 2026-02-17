using Msh.Api.Domain.Contracts.Search;
using Msh.Api.Domain.Interfaces.Builders;

namespace Msh.Api.Domain.Builders;

public class FacetBuilder : IFacetBuilder
{
    public List<FacetResponse> Build(IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> resultFacets,
                                     Dictionary<string, string> facets,
                                     IEnumerable<string>? activeFilters)
    {
        if (resultFacets is null)
            return default!;

        var activeFiltersSet = new HashSet<string>(activeFilters ?? [], StringComparer.OrdinalIgnoreCase);
        var facetsResult = new List<FacetResponse>(facets.Count);

        foreach (var facetKey in facets)
        {
            if (resultFacets.TryGetValue(facetKey.Key, out var items))
            {
                var mappedItems = MapFacetsToFilters(items, activeFiltersSet);
                if (mappedItems.Count > 0)
                {
                    facetsResult.Add(new FacetResponse(facetKey.Value, mappedItems));
                }
            }
        }

        return facetsResult;
    }

    private static List<FacetItemResponse> MapFacetsToFilters(IReadOnlyDictionary<string, int> items, HashSet<string> activeFilters)
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