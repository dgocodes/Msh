using System.Text.Json.Serialization;

namespace Msh.Api.Domain.Contracts.Search;

public record ProductResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("nome")] string Name,
    [property: JsonPropertyName("codigo_barras")] string Barcode,
    [property: JsonPropertyName("marca")] string Brand,
    [property: JsonPropertyName("lista")] string List,
    [property: JsonPropertyName("preco")] decimal Price,
    [property: JsonPropertyName("estoque")] int Stock);