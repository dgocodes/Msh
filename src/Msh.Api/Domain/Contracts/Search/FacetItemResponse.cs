using Msh.Api.Domain.Core;

namespace Msh.Api.Domain.Contracts.Search;

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
