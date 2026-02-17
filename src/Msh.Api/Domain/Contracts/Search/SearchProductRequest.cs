using Msh.Api.Domain.Core;

namespace Msh.Api.Domain.Contracts.Search;

public record SearchProductRequest(
    string Query,
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
        return $"search:{Query.ToLower().Trim()}:p{Page}:s{PageSize}:s{Sort ?? "none"}:f:{StringExtensions.GenerateCacheKey(filterString)}";
    }
}


//public class SearchProductRequestValidator : AbstractValidator<SearchProductRequest>
//{
//    public SearchProductRequestValidator()
//    {
//        // Regras para o termo de busca
//        RuleFor(x => x.Term)
//            .MinimumLength(3)
//            .WithMessage("O termo de busca deve ter no mínimo {MinLength} caracteres.")
//            .MaximumLength(100)
//            .WithMessage("O termo de busca deve ter no máximo {MaxLength} caracteres.");

//        // Regras para a Página
//        RuleFor(x => x.Page)
//            .LessThanOrEqualTo(20)
//            .WithMessage("O limite máximo de {ComparisonValue} páginas foi excedido. Refine sua busca.");

//        // Regras para o Tamanho da Página (PageSize)
//        RuleFor(x => x.PageSize)
//            .GreaterThan(0)
//            .WithMessage("O tamanho da página deve ser de pelo menos {ComparisonValue}.")

//            .LessThanOrEqualTo(100)
//            .WithMessage("Não é possível solicitar mais de {ComparisonValue} produtos por vez.");

//        // Regras para o Offset (Deep Paging Protection)
//        RuleFor(x => x.Offset)
//            .GreaterThanOrEqualTo(0)
//            .WithMessage("O deslocamento (offset) não pode ser negativo.")

//            .LessThanOrEqualTo(2000)
//            .WithMessage("A navegação é limitada aos primeiros {ComparisonValue} resultados. Refine os filtros.");
//    }
//}