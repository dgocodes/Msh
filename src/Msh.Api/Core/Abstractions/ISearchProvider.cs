using Msh.Api.Features.Products.SearchProduts;

namespace Msh.Api.Core.Abstractions;

public interface ISearchProvider
{
    Task<SearchProductResponse> SearchAsync(SearchProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse> GetDocumentAsync(string idProduto, CancellationToken cancellationToken = default);
}




