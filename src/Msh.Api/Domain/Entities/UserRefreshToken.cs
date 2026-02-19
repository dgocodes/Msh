using Msh.Api.Infra.Identity;

namespace Msh.Api.Domain.Entities;

public class UserRefreshToken
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }

    // Relacionamento com o Identity
    public ApplicationUser User { get; set; } = null!;
}