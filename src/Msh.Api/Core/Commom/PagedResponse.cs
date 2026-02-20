namespace Msh.Api.Core.Commom;

public record PagedResponse<T>(IReadOnlyList<T> Items, int TotalPages, int CurrentPage);