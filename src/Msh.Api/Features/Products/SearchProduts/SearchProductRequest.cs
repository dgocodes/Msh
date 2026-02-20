using Msh.Api.Core.Commom;

namespace Msh.Api.Features.Products.SearchProduts;


public record SearchProductRequest(
    string? Query = "",
    string? Sort = null,
    int Page = 1,
    int PageSize = 20,
    string? Filters = null)
{
    public int Offset => (Page - 1) * PageSize;

    // Cacheamos o processamento da string para não repetir o Split
    private readonly Lazy<string[]> _parsedFilters = new(() =>
        string.IsNullOrWhiteSpace(Filters)
            ? Array.Empty<string>()
            : Filters.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    );

    public IReadOnlyList<string> AppliedFilters() => _parsedFilters.Value;

    public string? BuildApplyFilters()
    {
        var filters = _parsedFilters.Value;
        if (filters.Length == 0) return null;

        // Escapa aspas simples para evitar erro no MeiliSearch
        // O formato final será: (filtros_dinamicos = 'A') AND (filtros_dinamicos = 'B')
        return string.Join(" AND ", filters.Select(f =>
            $"filtros_dinamicos = '{f.Replace("'", "\\'")}'"));
    }

    public string[]? BuildSort() => Sort switch
    {
        "mais-vendidos" => ["qtd_vendas:desc"],
        "menor-preco" => ["preco:asc"],
        "maior-preco" => ["preco:desc"],
        "nome" => ["nome:asc"],
        "destaque" => null,
        _ => null
    };

    public override string ToString()
    {
        // Ordenamos os filtros antes de gerar o hash para garantir que
        var sortedFilters = _parsedFilters.Value.OrderBy(f => f).ToList();
        var filterString = sortedFilters.Count > 0 ? string.Join(",", sortedFilters) : "none";
        return $"search:{Query?.ToLower().Trim()}:p{Page}:s{PageSize}:s{Sort ?? "none"}:f:{StringExtensions.GenerateCacheKey(filterString)}";
    }
}