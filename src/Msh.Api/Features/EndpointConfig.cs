namespace Msh.Api.Features;

public static class EndpointConfig
{
    public const string Base = "api";

    public const string TagProducts = "Produtos";
    public const string RouteProducts = $"/{Base}/produtos";
    public const string RouteProductById = "/api/produtos/{id}";

    public const string TagUsers = "Usuários";
    public const string RouteUsers = $"/{Base}/usuarios";

    public const string TagOrders = "Pedidos";
    public const string RouteOrders = $"/{Base}/pedidos";
}
