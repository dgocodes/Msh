namespace Msh.Api.Core.Entities;

public class CartItem
{
    public Guid Id { get; set; }
    public Guid SkuId { get; set; }
    public int Quantity { get; set; }
    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = null!;
}