using Meilisearch;
using Microsoft.Extensions.Options;
using Msh.Api.Domain.Contracts.Search;
using Msh.Api.Domain.Interfaces.Builders;
using Msh.Api.Domain.Interfaces.Providers;

namespace Msh.Api.Infra.Providers.Meili;

public class MeiliSearchProvider : ISearchProvider
{
    private readonly MeilisearchClient _client;
    private readonly Meilisearch.Index _index;
    private readonly MeiliSearchConfiguration _meiliSettings;
    private readonly IFacetBuilder _facetBuilder;
    private readonly ILogger<MeiliSearchProvider> _logger;

    public MeiliSearchProvider(MeilisearchClient client,
                               IOptions<MeiliSearchConfiguration> meiliSettings,
                               IFacetBuilder facetBuilder,
                               ILogger<MeiliSearchProvider> logger)
    {
        _client = client;
        _meiliSettings = meiliSettings.Value;
        _index = _client.Index(_meiliSettings.IndexName);
        _facetBuilder = facetBuilder;
        _logger = logger;
    }

    public async Task<SearchProductResponse> SearchAsync(SearchProductRequest request, CancellationToken cancellationToken = default)
    {
        var filters = request.BuildApplyFilters();

        var searchQuery = new SearchQuery
        {
            //Sort = BuildSort(criteria.Sort),
            Filter = filters,
            Limit = request.PageSize,
            Offset = request.Offset,
            AttributesToRetrieve = _meiliSettings.Configuration?.AttributesToRetrieve,
            Facets = _meiliSettings.Configuration?.GetCurrentFacets()
        };

        var searchResponse = await _index.SearchAsync<ProductResponse>(request.Query, searchQuery, cancellationToken); 
        var result = (SearchResult<ProductResponse>)searchResponse;

        var facets = _facetBuilder.Build(result.FacetDistribution, _meiliSettings.Configuration?.Facets ?? [], request.Filters);

        _logger.LogInformation(
            "Search performed | Term={Term} | Filters={Filters} | Page={Page} | PageSize={PageSize} | Hits={TotalHits} | Duration={Elapsed:0.0000}ms",
            request.Query,
            filters,
            request.Page,
            request.PageSize,
            result.EstimatedTotalHits,
            result.ProcessingTimeMs
        );

        return new SearchProductResponse(
            [.. (result.Hits ?? Enumerable.Empty<ProductResponse>())],
            facets,
            request.Page,
            request.PageSize,
            result.EstimatedTotalHits,
            result.ProcessingTimeMs
        );
    }
}



