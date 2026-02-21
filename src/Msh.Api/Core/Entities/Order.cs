using Msh.Api.Core.Enums;
using Msh.Api.Infra.Identity;

namespace Msh.Api.Core.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string UserId { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public EOrderStatus Status { get; set; } = EOrderStatus.Pending;

    // Endereço de entrega "congelado" (Snapshot)
    public string ShippingAddress { get; set; } = string.Empty;

    // Relacionamentos
    public ApplicationUser User { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new();
    public Payment? Payment { get; set; }
}
