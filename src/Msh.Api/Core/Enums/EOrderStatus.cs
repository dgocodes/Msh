namespace Msh.Api.Core.Enums;

public enum EOrderStatus
{
    Pending = 1,     // Aguardando pagamento
    Paid = 2,        // Pago
    Shipped = 3,     // Enviado
    Delivered = 4,   // Entregue
    Canceled = 5     // Cancelado
}