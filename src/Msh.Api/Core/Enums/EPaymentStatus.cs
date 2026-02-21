namespace Msh.Api.Core.Enums;

public enum EPaymentStatus
{
    Pending = 1,
    Authorized = 2,
    Settled = 3, // Dinheiro caiu na conta
    Refunded = 4,
    Failed = 5
}