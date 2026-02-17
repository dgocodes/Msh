using System.ComponentModel.DataAnnotations;

namespace Msh.Api.Infra.Providers.Meili;

public record MeiliSearchConfiguration
{
    public const string SectionName = "Meilisearch";
    public const string ClientName = "MeilisearchClient";

    [Required(ErrorMessage = "A URL do Meilisearch é obrigatória.")]
    [Url(ErrorMessage = "A URL do Meilisearch deve ser válida.")]
    public string Url { get; init; } = string.Empty;

    [Required(ErrorMessage = "A ApiKey do Meilisearch é obrigatória.")]
    public string ApiKey { get; init; } = string.Empty;

    [Required(ErrorMessage = "O nome do índice (IndexName) é obrigatório.")]
    public string IndexName { get; init; } = string.Empty;

    [Required]
    public MeilisearchSettings Configuration { get; init; } = new();
}

public record MeilisearchSettings
{
    public string[] RankingRules { get; init; } = [];
    public string[] SearchableAttributes { get; init; } = [];
    public string[] FilterableAttributes { get; init; } = [];
    public string[] SortableAttributes { get; init; } = [];

    [Required]
    [MinLength(1, ErrorMessage = "Defina ao menos um atributo para recuperar (AttributesToRetrieve).")]
    public string[] AttributesToRetrieve { get; init; } = [];

    public Dictionary<string, string> Facets { get; init; } = [];

    public string[] GetCurrentFacets() => [.. Facets.Select(x => x.Key)];
}