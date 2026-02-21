namespace Msh.Api.Core.Entities;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid OrderId { get; set; }
    public string SkuId { get; set; } = "";
    public int Quantity { get; set; }

    // O Preço unitário deve ser salvo aqui! (Snapshot)
    public decimal UnitPrice { get; set; }

    // Total do item (Quantidade * UnitPrice)
    public decimal SubTotal => Quantity * UnitPrice;

    // Relacionamentos
    public Order Order { get; set; } = null!;
}
