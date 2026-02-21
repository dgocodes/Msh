using Msh.Api.Core.Enums;

namespace Msh.Api.Core.Entities;

public class Payment
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid OrderId { get; set; }

    public EPaymentMethod Method { get; set; }
    public EPaymentStatus Status { get; set; }

    // ID retornado pelo Gateway (Stripe, MercadoPago, etc)
    public string? ExternalTransactionId { get; set; }

    public DateTime? PaidAt { get; set; }

    // Relacionamento
    public Order Order { get; set; } = null!;
}
