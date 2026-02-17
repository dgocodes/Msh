using Msh.Api.Domain.Core;

namespace Msh.Api.Domain.Contracts.Search;

public record SearchProductRequest(string Query,
                                 int Page = 1,
                                 int PageSize = 20,
                                 IEnumerable<string>? Filters = null) 
{
    public int Offset => (Page - 1) * PageSize;

    public string? BuildApplyFilters()
    {
        return Filters?.Any() is true
            ? string.Join(" AND ", Filters.Select(slug => $"filtros_dinamicos = '{slug}'"))
            : null;
    }

    public override string ToString()
    {
        var filterHash = Filters != null ? string.Join(",", Filters) : "none";
        return $"search:{Query}:p{Page}:l{PageSize}:f:{StringExtensions.GenerateCacheKey(filterHash)}";
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