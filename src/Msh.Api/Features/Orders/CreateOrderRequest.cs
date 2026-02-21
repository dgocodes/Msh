namespace Msh.Api.Features.Orders;

public record CreateOrderRequest(
    string ShippingAddress,
    List<CreateOrderItemRequest> Items)
{
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(ShippingAddress))
            return false;

        if (Items == null || Items.Count == 0)
            return false;

        return Items.All(item => 
            !string.IsNullOrWhiteSpace(item.SkuId) && 
            item.Quantity > 0);
    }
}

public record CreateOrderItemRequest(
    string SkuId,
    int Quantity);
