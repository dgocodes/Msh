using Msh.Api.Domain.Contracts.Search;
namespace Msh.Api.Domain.Interfaces.Providers;

public interface ISearchProvider
{
    Task<SearchProductResponse> SearchAsync(SearchProductRequest request, CancellationToken cancellationToken = default);
}




