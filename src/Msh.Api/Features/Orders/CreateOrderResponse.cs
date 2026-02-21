namespace Msh.Api.Features.Orders;

public record CreateOrderResponse(
    Guid OrderId,
    string Status,
    decimal TotalAmount,
    DateTime OrderDate,
    List<OrderItemResponse> Items);

public record OrderItemResponse(
    Guid ItemId,
    string SkuId,
    int Quantity,
    decimal UnitPrice,
    decimal SubTotal);
